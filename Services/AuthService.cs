using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Security;
using ESCOLA_API.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ESCOLA_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseViewModel?> LoginAsync(LoginRequestViewModel viewModel)
        {
            var email = viewModel.Email.Trim().ToLowerInvariant();
            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (usuario == null || !PasswordHasher.VerifyPassword(viewModel.Senha, usuario.Senha))
            {
                return null;
            }

            var expires = DateTime.UtcNow.AddMinutes(GetExpirationMinutes());
            return new AuthResponseViewModel
            {
                Token = GenerateToken(usuario, expires),
                ExpiraEm = expires,
                Usuario = usuario.ToSummary()!,
                DeveAlterarSenhaPadrao = DefaultPasswordPolicy.UsesDefaultPassword(usuario.Senha)
            };
        }

        public async Task<UsuarioSummaryViewModel?> GetUsuarioAtualAsync(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var idUsuario))
            {
                return null;
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            return usuario.ToSummary();
        }

        public async Task<UsuarioSummaryViewModel?> AlterarSenhaAsync(ClaimsPrincipal principal, AlterarSenhaViewModel viewModel)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var idUsuario))
            {
                return null;
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return null;
            }

            if (!PasswordHasher.VerifyPassword(viewModel.SenhaAtual, usuario.Senha))
            {
                throw new InvalidOperationException("Senha atual invalida.");
            }

            if (PasswordHasher.VerifyPassword(viewModel.NovaSenha, usuario.Senha))
            {
                throw new InvalidOperationException("A nova senha deve ser diferente da senha atual.");
            }

            usuario.Senha = PasswordHasher.HashPassword(viewModel.NovaSenha);
            await _context.SaveChangesAsync();

            return usuario.ToSummary();
        }

        public async Task<RedefinicaoSenhaSolicitadaViewModel> SolicitarRedefinicaoSenhaAsync(EsqueciSenhaViewModel viewModel)
        {
            var email = viewModel.Email.Trim().ToLowerInvariant();
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (usuario == null)
            {
                return new RedefinicaoSenhaSolicitadaViewModel
                {
                    UsuarioEncontrado = false
                };
            }

            var token = GeneratePasswordResetToken();
            var expiraEm = DateTime.UtcNow.AddMinutes(GetPasswordResetExpirationMinutes());

            usuario.ResetSenhaTokenHash = HashPasswordResetToken(token);
            usuario.ResetSenhaTokenCriadoEmUtc = DateTime.UtcNow;
            usuario.ResetSenhaTokenExpiraEmUtc = expiraEm;

            await _context.SaveChangesAsync();

            return new RedefinicaoSenhaSolicitadaViewModel
            {
                UsuarioEncontrado = true,
                TokenRedefinicao = token,
                ExpiraEmUtc = expiraEm
            };
        }

        public async Task<bool> RedefinirSenhaAsync(RedefinirSenhaViewModel viewModel)
        {
            var email = viewModel.Email.Trim().ToLowerInvariant();
            var tokenHash = HashPasswordResetToken(viewModel.Token.Trim());
            var now = DateTime.UtcNow;

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (usuario == null
                || string.IsNullOrWhiteSpace(usuario.ResetSenhaTokenHash)
                || usuario.ResetSenhaTokenHash != tokenHash
                || usuario.ResetSenhaTokenExpiraEmUtc == null
                || usuario.ResetSenhaTokenExpiraEmUtc <= now)
            {
                return false;
            }

            if (PasswordHasher.VerifyPassword(viewModel.NovaSenha, usuario.Senha))
            {
                throw new InvalidOperationException("A nova senha deve ser diferente da senha atual.");
            }

            usuario.Senha = PasswordHasher.HashPassword(viewModel.NovaSenha);
            usuario.ResetSenhaTokenHash = null;
            usuario.ResetSenhaTokenCriadoEmUtc = null;
            usuario.ResetSenhaTokenExpiraEmUtc = null;

            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateToken(Usuario usuario, DateTime expires)
        {
            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("Jwt:Key nao configurado.");
            }

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                new(JwtRegisteredClaimNames.Email, usuario.Email),
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.Nome),
                new("id_perfil", usuario.IdPerfil.ToString())
            };

            if (!string.IsNullOrWhiteSpace(usuario.Perfil?.DescricaoPerfil))
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Perfil.DescricaoPerfil));
            }

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetExpirationMinutes()
        {
            return int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var minutes)
                ? minutes
                : 120;
        }

        private int GetPasswordResetExpirationMinutes()
        {
            return int.TryParse(_configuration["PasswordReset:ExpirationMinutes"], out var minutes) && minutes > 0
                ? minutes
                : 30;
        }

        private static string GeneratePasswordResetToken()
        {
            return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        }

        private static string HashPasswordResetToken(string token)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hash);
        }
    }
}

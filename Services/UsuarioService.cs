using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Security;
using ESCOLA_API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly DataContext _context;

        public UsuarioService(DataContext context)
        {
            _context = context;
        }

        public async Task<UsuarioSummaryViewModel[]> GetAllAsync(ClaimsPrincipal principal)
        {
            var query = _context.Usuarios
                .Include(usuario => usuario.Perfil)
                .AsNoTracking();

            if (IsProfessor(principal))
            {
                query = query.Where(usuario =>
                    usuario.IdPerfil == PerfilSistema.AlunoId || usuario.IdPerfil == PerfilSistema.ProfessorId);
            }
            else if (!IsAdministrador(principal))
            {
                var usuarioAtualId = GetUsuarioAtualId(principal);
                query = query.Where(usuario => usuario.IdUsuario == usuarioAtualId);
            }

            return await query
                .OrderBy(usuario => usuario.Nome)
                .Select(usuario => new UsuarioSummaryViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Telefone = usuario.Telefone,
                    FotoPerfilUrl = usuario.FotoPerfilUrl,
                    IdPerfil = usuario.IdPerfil,
                    DescricaoPerfil = usuario.Perfil == null ? string.Empty : usuario.Perfil.DescricaoPerfil,
                    TipoUsuario = PerfilSistema.ObterDescricaoPorId(usuario.IdPerfil)
                })
                .ToArrayAsync();
        }

        public async Task<UsuarioSummaryViewModel?> GetByIdAsync(int usuarioId, ClaimsPrincipal principal)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);

            if (usuario != null && !PodeConsultar(principal, usuario))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar este cadastro.");
            }

            return usuario.ToSummary();
        }

        public async Task<UsuarioSummaryViewModel> AddAsync(UsuarioCreateViewModel viewModel, ClaimsPrincipal principal)
        {
            var email = NormalizeEmail(viewModel.Email);
            var emailJaCadastrado = await _context.Usuarios
                .AnyAsync(usuario => usuario.Email.ToLower() == email);

            if (emailJaCadastrado)
            {
                throw new InvalidOperationException("Email ja cadastrado.");
            }

            if (!PerfilSistema.TryObterPerfilId(viewModel.TipoUsuario, out var idPerfil))
            {
                throw new InvalidOperationException("Tipo de usuario informado nao existe.");
            }

            ValidarPermissaoCadastro(principal, idPerfil);

            var adminId = GetUsuarioAtualId(principal);
            var admin = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == adminId);

            var usuario = new Usuario
            {
                Nome = viewModel.Nome.Trim(),
                Email = email,
                Telefone = viewModel.Telefone.Trim(),
                Senha = PasswordHasher.HashPassword(DefaultPasswordPolicy.DefaultPassword),
                IdPerfil = idPerfil,
                IdUsuarioCriador = admin?.IdUsuario,
                NomeUsuarioCriador = admin?.Nome
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            CriarNotificacaoCadastro(usuario);
            await _context.SaveChangesAsync();

            var created = await _context.Usuarios
                .Include(u => u.Perfil)
                .AsNoTracking()
                .FirstAsync(u => u.IdUsuario == usuario.IdUsuario);

            return created.ToSummary()!;
        }

        public async Task<UsuarioSummaryViewModel?> UpdateAsync(int usuarioId, UsuarioUpdateViewModel viewModel, ClaimsPrincipal principal)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);

            if (usuario == null)
            {
                return null;
            }

            var email = NormalizeEmail(viewModel.Email);
            var emailJaCadastrado = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario != usuarioId && u.Email.ToLower() == email);

            if (emailJaCadastrado)
            {
                throw new InvalidOperationException("Email ja cadastrado.");
            }

            var alterarTipoUsuario = !string.IsNullOrWhiteSpace(viewModel.TipoUsuario);
            var idPerfil = usuario.IdPerfil;

            if (alterarTipoUsuario && !PerfilSistema.TryObterPerfilId(viewModel.TipoUsuario, out idPerfil))
            {
                throw new InvalidOperationException("Tipo de usuario informado nao existe.");
            }

            ValidarPermissaoAtualizacao(principal, usuario, alterarTipoUsuario);

            var atualizacaoPropria = usuario.IdUsuario == GetUsuarioAtualId(principal) && !IsAdministrador(principal);
            var dadosAnteriores = new DadosPerfilUsuario(usuario.Nome, usuario.Email, usuario.Telefone, usuario.IdPerfil);
            var dadosCadastraisAlterados =
                usuario.Nome != viewModel.Nome.Trim()
                || usuario.Email != email
                || usuario.Telefone != viewModel.Telefone.Trim();

            usuario.Nome = viewModel.Nome.Trim();
            usuario.Email = email;
            usuario.Telefone = viewModel.Telefone.Trim();

            if (alterarTipoUsuario)
            {
                usuario.IdPerfil = idPerfil;
            }

            if (atualizacaoPropria && dadosCadastraisAlterados)
            {
                await CriarNotificacaoDadosAtualizadosAsync(usuario, dadosAnteriores);
            }

            await _context.SaveChangesAsync();

            var updated = await _context.Usuarios
                .Include(u => u.Perfil)
                .AsNoTracking()
                .FirstAsync(u => u.IdUsuario == usuarioId);

            return updated.ToSummary();
        }

        public async Task<bool> DeleteAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);

            if (usuario == null)
            {
                return false;
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PerfilViewModel[]> GetPerfisAsync(ClaimsPrincipal principal)
        {
            if (!IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar perfis para cadastro.");
            }

            return await _context.Perfis
                .AsNoTracking()
                .OrderBy(perfil => perfil.IdPerfil)
                .Select(perfil => new PerfilViewModel
                {
                    IdPerfil = perfil.IdPerfil,
                    DescricaoPerfil = perfil.DescricaoPerfil
                })
                .ToArrayAsync();
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private static void ValidarPermissaoCadastro(ClaimsPrincipal principal, int idPerfil)
        {
            if (IsAdministrador(principal))
            {
                return;
            }

            throw new UnauthorizedAccessException("Usuario nao autorizado a cadastrar este tipo de usuario.");
        }

        private static void ValidarPermissaoAtualizacao(ClaimsPrincipal principal, Usuario usuario, bool alterarTipoUsuario)
        {
            if (IsAdministrador(principal))
            {
                return;
            }

            if (!alterarTipoUsuario && usuario.IdUsuario == GetUsuarioAtualId(principal))
            {
                return;
            }

            throw new UnauthorizedAccessException("Usuario nao autorizado a alterar este cadastro.");
        }

        private static bool PodeConsultar(ClaimsPrincipal principal, Usuario usuario)
        {
            if (IsAdministrador(principal))
            {
                return true;
            }

            var usuarioAtualId = GetUsuarioAtualId(principal);

            return usuario.IdUsuario == usuarioAtualId
                || (IsProfessor(principal)
                    && (usuario.IdPerfil == PerfilSistema.AlunoId || usuario.IdPerfil == PerfilSistema.ProfessorId));
        }

        private void CriarNotificacaoCadastro(Usuario usuario)
        {
            var nomeCriador = string.IsNullOrWhiteSpace(usuario.NomeUsuarioCriador)
                ? "Administrador"
                : usuario.NomeUsuarioCriador;

            _context.Notificacoes.Add(new Notificacao
            {
                IdUsuario = usuario.IdUsuario,
                Tipo = "CadastroUsuario",
                Titulo = "Cadastro criado",
                Mensagem = $"Seu cadastro foi criado por {nomeCriador}. Dados cadastrados: Nome: {usuario.Nome}; E-mail: {usuario.Email}; Telefone: {usuario.Telefone}; Perfil: {PerfilSistema.ObterDescricaoPorId(usuario.IdPerfil)}. Voce pode editar seus dados quando achar necessario.",
                Link = $"/usuarios/{usuario.IdUsuario}",
                CriadaEmUtc = DateTime.UtcNow
            });
        }

        private async Task CriarNotificacaoDadosAtualizadosAsync(Usuario usuario, DadosPerfilUsuario dadosAnteriores)
        {
            var administradoresIds = await _context.Usuarios
                .AsNoTracking()
                .Where(item => item.IdPerfil == PerfilSistema.AdministradorId)
                .Select(item => item.IdUsuario)
                .ToArrayAsync();

            if (administradoresIds.Length == 0)
            {
                return;
            }

            var mensagem = $"O usuario {usuario.Nome} alterou seus dados de perfil. "
                + $"Dados anteriores: Nome: {dadosAnteriores.Nome}; E-mail: {dadosAnteriores.Email}; Telefone: {dadosAnteriores.Telefone}; Perfil: {PerfilSistema.ObterDescricaoPorId(dadosAnteriores.IdPerfil)}. "
                + $"Dados atuais: Nome: {usuario.Nome}; E-mail: {usuario.Email}; Telefone: {usuario.Telefone}; Perfil: {PerfilSistema.ObterDescricaoPorId(usuario.IdPerfil)}.";

            foreach (var administradorId in administradoresIds)
            {
                _context.Notificacoes.Add(new Notificacao
                {
                    IdUsuario = administradorId,
                    Tipo = "DadosUsuarioAtualizados",
                    Titulo = "Dados do usuario atualizados",
                    Mensagem = mensagem,
                    Link = $"/usuarios/{usuario.IdUsuario}",
                    CriadaEmUtc = DateTime.UtcNow
                });
            }
        }

        private static bool IsAdministrador(ClaimsPrincipal principal)
        {
            return principal.IsInRole(PerfilSistema.Administrador);
        }

        private static bool IsProfessor(ClaimsPrincipal principal)
        {
            return principal.IsInRole(PerfilSistema.Professor);
        }

        private static int GetUsuarioAtualId(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var idUsuario) ? idUsuario : 0;
        }

        private sealed record DadosPerfilUsuario(string Nome, string Email, string Telefone, int IdPerfil);
    }
}

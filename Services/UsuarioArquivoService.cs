using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public class UsuarioArquivoService : IUsuarioArquivoService
    {
        private const long FotoMaxBytes = 2 * 1024 * 1024;
        private const long PdfMaxBytes = 10 * 1024 * 1024;
        private readonly DataContext _context;
        private readonly IUsuarioArquivoStorage _storage;

        public UsuarioArquivoService(DataContext context, IUsuarioArquivoStorage storage)
        {
            _context = context;
            _storage = storage;
        }

        public async Task<UsuarioSummaryViewModel?> UploadFotoAsync(int usuarioId, IFormFile arquivo, ClaimsPrincipal principal)
        {
            var usuario = await _context.Usuarios
                .Include(item => item.Perfil)
                .FirstOrDefaultAsync(item => item.IdUsuario == usuarioId);

            if (usuario == null)
            {
                return null;
            }

            ValidarPermissaoArquivo(principal, usuario, permiteProfessorEditarAluno: true);
            ValidarArquivo(arquivo, FotoMaxBytes, new[] { ".jpg", ".jpeg", ".png", ".webp" }, new[] { "image/jpeg", "image/png", "image/webp" });

            await _storage.RemoverAsync(null, usuario.FotoPerfilUrl);
            var arquivoSalvo = await _storage.SalvarAsync(usuarioId, "foto", arquivo);
            usuario.FotoPerfilUrl = arquivoSalvo.Url;
            await _context.SaveChangesAsync();

            return usuario.ToSummary();
        }

        public async Task<UsuarioArquivoViewModel[]> GetArquivosAsync(int usuarioId, ClaimsPrincipal principal)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.IdUsuario == usuarioId);

            if (usuario == null)
            {
                return Array.Empty<UsuarioArquivoViewModel>();
            }

            ValidarPermissaoArquivo(principal, usuario, permiteProfessorEditarAluno: false);

            return await _context.UsuarioArquivos
                .AsNoTracking()
                .Where(arquivo => arquivo.IdUsuario == usuarioId)
                .OrderByDescending(arquivo => arquivo.CriadoEmUtc)
                .Select(arquivo => new UsuarioArquivoViewModel
                {
                    IdArquivo = arquivo.IdArquivo,
                    IdUsuario = arquivo.IdUsuario,
                    NomeBlob = arquivo.NomeBlob,
                    TipoArquivo = arquivo.TipoArquivo,
                    NomeOriginal = arquivo.NomeOriginal,
                    Url = arquivo.Url,
                    ContentType = arquivo.ContentType,
                    TamanhoBytes = arquivo.TamanhoBytes,
                    CriadoEmUtc = arquivo.CriadoEmUtc
                })
                .ToArrayAsync();
        }

        public async Task<UsuarioArquivoViewModel> UploadCertificadoAsync(int usuarioId, IFormFile arquivo, ClaimsPrincipal principal)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(item => item.IdUsuario == usuarioId);

            if (usuario == null)
            {
                throw new InvalidOperationException("Usuario nao encontrado.");
            }

            if (usuario.IdPerfil != PerfilSistema.ProfessorId)
            {
                throw new InvalidOperationException("Certificados podem ser enviados apenas para professores.");
            }

            ValidarPermissaoArquivo(principal, usuario, permiteProfessorEditarAluno: false);
            ValidarArquivo(arquivo, PdfMaxBytes, new[] { ".pdf" }, new[] { "application/pdf" });

            var arquivoSalvo = await _storage.SalvarAsync(usuarioId, "certificados", arquivo);
            var entity = new UsuarioArquivo
            {
                IdUsuario = usuarioId,
                TipoArquivo = "Certificado",
                NomeOriginal = Path.GetFileName(arquivo.FileName),
                NomeBlob = arquivoSalvo.NomeBlob,
                Url = arquivoSalvo.Url,
                ContentType = arquivo.ContentType,
                TamanhoBytes = arquivo.Length,
                CriadoEmUtc = DateTime.UtcNow
            };

            _context.UsuarioArquivos.Add(entity);
            await _context.SaveChangesAsync();

            return entity.ToViewModel();
        }

        public async Task<bool> DeleteArquivoAsync(int usuarioId, int arquivoId, ClaimsPrincipal principal)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.IdUsuario == usuarioId);

            if (usuario == null)
            {
                return false;
            }

            ValidarPermissaoArquivo(principal, usuario, permiteProfessorEditarAluno: false);

            var arquivo = await _context.UsuarioArquivos
                .FirstOrDefaultAsync(item => item.IdArquivo == arquivoId && item.IdUsuario == usuarioId);

            if (arquivo == null)
            {
                return false;
            }

            await _storage.RemoverAsync(arquivo.NomeBlob, arquivo.Url);
            _context.UsuarioArquivos.Remove(arquivo);
            await _context.SaveChangesAsync();
            return true;
        }

        private static void ValidarArquivo(IFormFile arquivo, long maxBytes, string[] extensoes, string[] contentTypes)
        {
            if (arquivo.Length == 0)
            {
                throw new InvalidOperationException("Arquivo vazio.");
            }

            if (arquivo.Length > maxBytes)
            {
                throw new InvalidOperationException("Arquivo excede o tamanho maximo permitido.");
            }

            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            if (!extensoes.Contains(extensao))
            {
                throw new InvalidOperationException("Tipo de arquivo nao permitido.");
            }

            if (!contentTypes.Contains(arquivo.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Tipo de arquivo nao permitido.");
            }
        }

        private static void ValidarPermissaoArquivo(ClaimsPrincipal principal, Usuario usuario, bool permiteProfessorEditarAluno)
        {
            if (principal.IsInRole(PerfilSistema.Administrador))
            {
                return;
            }

            var usuarioAtualId = GetUsuarioAtualId(principal);
            if (usuario.IdUsuario == usuarioAtualId)
            {
                return;
            }

            if (permiteProfessorEditarAluno && principal.IsInRole(PerfilSistema.Professor) && usuario.IdPerfil == PerfilSistema.AlunoId)
            {
                return;
            }

            throw new UnauthorizedAccessException("Usuario nao autorizado a gerenciar arquivos deste cadastro.");
        }

        private static int GetUsuarioAtualId(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var idUsuario) ? idUsuario : 0;
        }
    }
}

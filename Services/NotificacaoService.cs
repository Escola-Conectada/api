using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public class NotificacaoService : INotificacaoService
    {
        private readonly DataContext _context;

        public NotificacaoService(DataContext context)
        {
            _context = context;
        }

        public async Task<NotificacaoViewModel[]> GetMinhasAsync(ClaimsPrincipal principal)
        {
            var usuarioId = GetUsuarioAtualId(principal);

            return await _context.Notificacoes
                .AsNoTracking()
                .Where(notificacao => notificacao.IdUsuario == usuarioId)
                .OrderByDescending(notificacao => notificacao.CriadaEmUtc)
                .Select(notificacao => new NotificacaoViewModel
                {
                    IdNotificacao = notificacao.IdNotificacao,
                    IdUsuario = notificacao.IdUsuario,
                    Tipo = notificacao.Tipo,
                    Titulo = notificacao.Titulo,
                    Mensagem = notificacao.Mensagem,
                    Link = notificacao.Link,
                    IdCadernetaDigital = notificacao.IdCadernetaDigital,
                    IdDisciplina = notificacao.IdDisciplina,
                    NomeDisciplina = notificacao.NomeDisciplina,
                    MediaAritmetica = notificacao.MediaAritmetica,
                    Situacao = notificacao.Situacao,
                    CorSituacao = notificacao.CorSituacao,
                    Lida = notificacao.Lida,
                    CriadaEmUtc = notificacao.CriadaEmUtc,
                    LidaEmUtc = notificacao.LidaEmUtc
                })
                .ToArrayAsync();
        }

        public async Task<int> CountNaoLidasAsync(ClaimsPrincipal principal)
        {
            var usuarioId = GetUsuarioAtualId(principal);

            return await _context.Notificacoes
                .AsNoTracking()
                .CountAsync(notificacao => notificacao.IdUsuario == usuarioId && !notificacao.Lida);
        }

        public async Task<NotificacaoViewModel> AddAsync(NotificacaoCreateViewModel viewModel, ClaimsPrincipal principal)
        {
            if (!IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a enviar notificacoes.");
            }

            var destinatario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(usuario => usuario.IdUsuario == viewModel.IdUsuario);

            if (destinatario == null)
            {
                throw new InvalidOperationException("Usuario destinatario nao encontrado.");
            }

            var notificacao = new Notificacao
            {
                IdUsuario = destinatario.IdUsuario,
                Tipo = string.IsNullOrWhiteSpace(viewModel.Tipo) ? "Geral" : viewModel.Tipo.Trim(),
                Titulo = viewModel.Titulo.Trim(),
                Mensagem = viewModel.Mensagem.Trim(),
                Link = string.IsNullOrWhiteSpace(viewModel.Link) ? null : viewModel.Link.Trim(),
                CriadaEmUtc = DateTime.UtcNow
            };

            if (string.IsNullOrWhiteSpace(notificacao.Titulo) || string.IsNullOrWhiteSpace(notificacao.Mensagem))
            {
                throw new InvalidOperationException("Informe titulo e mensagem da notificacao.");
            }

            _context.Notificacoes.Add(notificacao);
            await _context.SaveChangesAsync();

            return notificacao.ToViewModel();
        }

        public async Task<NotificacaoViewModel?> MarcarComoLidaAsync(int notificacaoId, ClaimsPrincipal principal)
        {
            var usuarioId = GetUsuarioAtualId(principal);
            var notificacao = await _context.Notificacoes
                .FirstOrDefaultAsync(item => item.IdNotificacao == notificacaoId && item.IdUsuario == usuarioId);

            if (notificacao == null)
            {
                return null;
            }

            if (!notificacao.Lida)
            {
                notificacao.Lida = true;
                notificacao.LidaEmUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return notificacao.ToViewModel();
        }

        public async Task<int> MarcarTodasComoLidasAsync(ClaimsPrincipal principal)
        {
            var usuarioId = GetUsuarioAtualId(principal);
            var notificacoes = await _context.Notificacoes
                .Where(notificacao => notificacao.IdUsuario == usuarioId && !notificacao.Lida)
                .ToArrayAsync();

            foreach (var notificacao in notificacoes)
            {
                notificacao.Lida = true;
                notificacao.LidaEmUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return notificacoes.Length;
        }

        private static bool IsAdministrador(ClaimsPrincipal principal)
        {
            return principal.IsInRole(PerfilSistema.Administrador);
        }

        private static int GetUsuarioAtualId(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var idUsuario) ? idUsuario : 0;
        }
    }
}

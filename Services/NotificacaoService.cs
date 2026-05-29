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

            var notificacoes = await _context.Notificacoes
                .AsNoTracking()
                .Where(notificacao => notificacao.IdUsuario == usuarioId)
                .OrderByDescending(notificacao => notificacao.CriadaEmUtc)
                .ToArrayAsync();

            return notificacoes
                .Select(notificacao => notificacao.ToViewModel())
                .ToArray();
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

            ValidarNotificacao(notificacao.Titulo, notificacao.Mensagem);

            _context.Notificacoes.Add(notificacao);
            await _context.SaveChangesAsync();

            return notificacao.ToViewModel();
        }

        public async Task<NotificacaoEnvioViewModel> AddParaPerfisAsync(
            NotificacaoPerfisCreateViewModel viewModel,
            ClaimsPrincipal principal)
        {
            if (!IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a enviar notificacoes.");
            }

            var titulo = viewModel.Titulo.Trim();
            var mensagem = viewModel.Mensagem.Trim();
            ValidarNotificacao(titulo, mensagem);

            var idsPerfis = await ResolverIdsPerfisAsync(viewModel);
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Where(usuario => idsPerfis.Contains(usuario.IdPerfil))
                .Select(usuario => usuario.IdUsuario)
                .ToArrayAsync();

            if (usuarios.Length == 0)
            {
                throw new InvalidOperationException("Nenhum usuario encontrado para os perfis informados.");
            }

            var criadaEmUtc = DateTime.UtcNow;
            var notificacoes = usuarios
                .Distinct()
                .Select(idUsuario => new Notificacao
                {
                    IdUsuario = idUsuario,
                    Tipo = string.IsNullOrWhiteSpace(viewModel.Tipo) ? "Geral" : viewModel.Tipo.Trim(),
                    Titulo = titulo,
                    Mensagem = mensagem,
                    Link = string.IsNullOrWhiteSpace(viewModel.Link) ? null : viewModel.Link.Trim(),
                    CriadaEmUtc = criadaEmUtc
                })
                .ToArray();

            _context.Notificacoes.AddRange(notificacoes);
            await _context.SaveChangesAsync();

            return new NotificacaoEnvioViewModel
            {
                Total = notificacoes.Length,
                Notificacoes = notificacoes.Select(notificacao => notificacao.ToViewModel()).ToArray()
            };
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

        private async Task<int[]> ResolverIdsPerfisAsync(NotificacaoPerfisCreateViewModel viewModel)
        {
            if (viewModel.TodosOsPerfis
                || (viewModel.IdsPerfis.Length == 0
                    && viewModel.TiposUsuario.All(string.IsNullOrWhiteSpace)))
            {
                return await _context.Perfis
                    .AsNoTracking()
                    .Select(perfil => perfil.IdPerfil)
                    .ToArrayAsync();
            }

            var idsPerfis = new HashSet<int>(
                viewModel.IdsPerfis.Where(idPerfil => idPerfil > 0));

            foreach (var tipoUsuario in viewModel.TiposUsuario.Where(tipo => !string.IsNullOrWhiteSpace(tipo)))
            {
                if (!PerfilSistema.TryObterPerfilId(tipoUsuario, out var idPerfil))
                {
                    throw new InvalidOperationException($"Perfil destinatario invalido: {tipoUsuario}.");
                }

                idsPerfis.Add(idPerfil);
            }

            if (idsPerfis.Count == 0)
            {
                throw new InvalidOperationException("Informe ao menos um perfil destinatario.");
            }

            var idsExistentes = await _context.Perfis
                .AsNoTracking()
                .Where(perfil => idsPerfis.Contains(perfil.IdPerfil))
                .Select(perfil => perfil.IdPerfil)
                .ToArrayAsync();

            if (idsExistentes.Length != idsPerfis.Count)
            {
                throw new InvalidOperationException("Um ou mais perfis destinatarios nao foram encontrados.");
            }

            return idsExistentes;
        }

        private static void ValidarNotificacao(string titulo, string mensagem)
        {
            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(mensagem))
            {
                throw new InvalidOperationException("Informe titulo e mensagem da notificacao.");
            }
        }

        private static int GetUsuarioAtualId(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var idUsuario) ? idUsuario : 0;
        }
    }
}

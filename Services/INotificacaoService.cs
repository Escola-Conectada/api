using System.Security.Claims;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public interface INotificacaoService
    {
        Task<NotificacaoViewModel[]> GetMinhasAsync(ClaimsPrincipal principal, int pagina = 1, int tamanhoPagina = 20);
        Task<int> CountNaoLidasAsync(ClaimsPrincipal principal);
        Task<NotificacaoViewModel> AddAsync(NotificacaoCreateViewModel viewModel, ClaimsPrincipal principal);
        Task<NotificacaoEnvioViewModel> AddParaPerfisAsync(NotificacaoPerfisCreateViewModel viewModel, ClaimsPrincipal principal);
        Task<NotificacaoViewModel?> MarcarComoLidaAsync(int notificacaoId, ClaimsPrincipal principal);
        Task<int> MarcarTodasComoLidasAsync(ClaimsPrincipal principal);
    }
}

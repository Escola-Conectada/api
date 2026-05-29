using System.Security.Claims;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseViewModel?> LoginAsync(LoginRequestViewModel viewModel);
        Task<UsuarioSummaryViewModel?> GetUsuarioAtualAsync(ClaimsPrincipal principal);
        Task<UsuarioSummaryViewModel?> AlterarSenhaAsync(ClaimsPrincipal principal, AlterarSenhaViewModel viewModel);
        Task<RedefinicaoSenhaSolicitadaViewModel> SolicitarRedefinicaoSenhaAsync(EsqueciSenhaViewModel viewModel);
        Task<bool> RedefinirSenhaAsync(RedefinirSenhaViewModel viewModel);
    }
}

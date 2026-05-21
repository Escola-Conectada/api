using System.Security.Claims;
using form_API.ViewModels;

namespace form_API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseViewModel?> LoginAsync(LoginRequestViewModel viewModel);
        Task<UsuarioSummaryViewModel?> GetUsuarioAtualAsync(ClaimsPrincipal principal);
        Task<UsuarioSummaryViewModel?> AlterarSenhaAsync(ClaimsPrincipal principal, AlterarSenhaViewModel viewModel);
    }
}

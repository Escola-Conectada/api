using ESCOLA_API.ViewModels;
using System.Security.Claims;

namespace ESCOLA_API.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioSummaryViewModel[]> GetAllAsync(ClaimsPrincipal principal);
        Task<UsuarioSummaryViewModel?> GetByIdAsync(int usuarioId, ClaimsPrincipal principal);
        Task<UsuarioSummaryViewModel> AddAsync(UsuarioCreateViewModel viewModel, ClaimsPrincipal principal);
        Task<UsuarioSummaryViewModel?> UpdateAsync(int usuarioId, UsuarioUpdateViewModel viewModel, ClaimsPrincipal principal);
        Task<bool> DeleteAsync(int usuarioId);
        Task<PerfilViewModel[]> GetPerfisAsync(ClaimsPrincipal principal);
    }
}

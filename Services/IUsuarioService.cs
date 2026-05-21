using form_API.ViewModels;

namespace form_API.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioSummaryViewModel[]> GetAllAsync();
        Task<UsuarioSummaryViewModel?> GetByIdAsync(int usuarioId);
        Task<UsuarioSummaryViewModel> AddAsync(UsuarioCreateViewModel viewModel);
        Task<UsuarioSummaryViewModel?> UpdateAsync(int usuarioId, UsuarioCreateViewModel viewModel);
        Task<bool> DeleteAsync(int usuarioId);
        Task<PerfilViewModel[]> GetPerfisAsync();
    }
}

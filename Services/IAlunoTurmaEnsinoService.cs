using System.Security.Claims;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public interface IAlunoTurmaEnsinoService
    {
        Task<AlunoTurmaEnsinoViewModel[]> GetAllAsync(ClaimsPrincipal principal);
        Task<AlunoTurmaEnsinoViewModel?> GetByIdAsync(int matriculaId, ClaimsPrincipal principal);
        Task<AlunoTurmaEnsinoViewModel> AddAsync(AlunoTurmaEnsinoCreateUpdateViewModel viewModel, ClaimsPrincipal principal);
        Task<AlunoTurmaEnsinoViewModel?> UpdateAsync(int matriculaId, AlunoTurmaEnsinoCreateUpdateViewModel viewModel, ClaimsPrincipal principal);
        Task<bool> DeleteAsync(int matriculaId, ClaimsPrincipal principal);
    }
}

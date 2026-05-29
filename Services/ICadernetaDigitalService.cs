using System.Security.Claims;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public interface ICadernetaDigitalService
    {
        Task<CadernetaDigitalViewModel[]> GetAllAsync(ClaimsPrincipal principal);
        Task<CadernetaDigitalViewModel?> GetByIdAsync(int cadernetaId, ClaimsPrincipal principal);
        Task<CadernetaDigitalViewModel> AddAsync(CadernetaDigitalCreateUpdateViewModel viewModel, ClaimsPrincipal principal);
        Task<CadernetaDigitalViewModel?> UpdateAsync(int cadernetaId, CadernetaDigitalCreateUpdateViewModel viewModel, ClaimsPrincipal principal);
        Task<bool> DeleteAsync(int cadernetaId, ClaimsPrincipal principal);
        Task<DisciplinaViewModel[]> GetDisciplinasAsync(ClaimsPrincipal principal);
        Task<TipoEnsinoCurricularViewModel[]> GetEstruturaEnsinoAsync(ClaimsPrincipal principal);
        Task<DisciplinaViewModel> AddDisciplinaAsync(DisciplinaCreateUpdateViewModel viewModel, ClaimsPrincipal principal);
        Task<DisciplinaViewModel?> UpdateDisciplinaAsync(int disciplinaId, DisciplinaCreateUpdateViewModel viewModel, ClaimsPrincipal principal);
        Task<bool> DeleteDisciplinaAsync(int disciplinaId, ClaimsPrincipal principal);
    }
}

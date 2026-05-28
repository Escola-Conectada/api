using System.Security.Claims;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public interface IDisciplinaEventoService
    {
        Task<DisciplinaEventoViewModel[]> GetEventosAsync(ClaimsPrincipal principal, int? idDisciplina, int? ano, int? mes);
        Task<DisciplinaEventoViewModel> AddAsync(int disciplinaId, DisciplinaEventoCreateUpdateViewModel viewModel, ClaimsPrincipal principal);
        Task<DisciplinaEventoViewModel?> UpdateAsync(int disciplinaId, int eventoId, DisciplinaEventoCreateUpdateViewModel viewModel, ClaimsPrincipal principal);
        Task<bool> DeleteAsync(int disciplinaId, int eventoId, ClaimsPrincipal principal);
    }
}

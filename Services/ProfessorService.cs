using System.Linq;
using System.Threading.Tasks;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public class ProfessorService : IProfessorService
    {
        private readonly IRepository _repo;

        public ProfessorService(IRepository repo)
        {
            _repo = repo;
        }

        public async Task<ProfessorViewModel[]> GetAllAsync(bool includeAlunos = false)
        {
            var professores = await _repo.GetAllProfessoresAsync(includeAlunos);
            return professores.Select(p => p.ToViewModel()).ToArray();
        }

        public async Task<ProfessorViewModel?> GetByIdAsync(int professorId, bool includeAlunos = false)
        {
            var professor = await _repo.GetProfessorAsyncById(professorId, includeAlunos);
            return professor?.ToViewModel();
        }

        public async Task<ProfessorViewModel> AddAsync(ProfessorCreateEditViewModel viewModel)
        {
            await ValidarProfessorAsync(viewModel, null);
            var entity = viewModel.ToModel();
            _repo.Add(entity);
            await _repo.SaveChangesAsync();
            var created = await _repo.GetProfessorAsyncById(entity.Id, true);
            return created?.ToViewModel() ?? new ProfessorViewModel { Id = entity.Id, Nome = entity.Nome };
        }

        public async Task<ProfessorViewModel?> UpdateAsync(int professorId, ProfessorCreateEditViewModel viewModel)
        {
            var professor = await _repo.GetProfessorAsyncById(professorId, false);
            if (professor == null)
            {
                return null;
            }

            await ValidarProfessorAsync(viewModel, professorId);
            professor.Nome = viewModel.Nome;
            professor.IdUsuario = viewModel.IdUsuario;
            _repo.Update(professor);
            await _repo.SaveChangesAsync();
            var updated = await _repo.GetProfessorAsyncById(professorId, true);
            return updated?.ToViewModel();
        }

        public async Task<bool> DeleteAsync(int professorId)
        {
            var professor = await _repo.GetProfessorAsyncById(professorId, false);
            if (professor == null)
            {
                return false;
            }

            _repo.Delete(professor);
            return await _repo.SaveChangesAsync();
        }

        private async Task ValidarProfessorAsync(ProfessorCreateEditViewModel viewModel, int? professorId)
        {
            if (!await _repo.UsuarioExistsWithPerfilAsync(viewModel.IdUsuario, PerfilSistema.ProfessorId))
            {
                throw new InvalidOperationException("Usuario de professor nao encontrado.");
            }

            if (await _repo.ProfessorUsuarioInUseAsync(viewModel.IdUsuario, professorId))
            {
                throw new InvalidOperationException("Este usuario ja esta vinculado a outro professor.");
            }
        }
    }
}

using System.Threading.Tasks;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Moq;
using Xunit;

namespace ESCOLA_API.Tests.Services
{
    public class AlunoServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsAlunoViewModels()
        {
            var alunos = new[]
            {
                new Aluno
                {
                    Id = 1,
                    Nome = "Maria",
                    Sobrenome = "Solano",
                    DataNasc = "25/02/1982",
                    ProfessorId = 1,
                    IdUsuario = 12,
                    Professor = new Professor { Id = 1, Nome = "Vinicius" }
                }
            };

            var repository = new Mock<IRepository>();
            repository.Setup(r => r.GetAllAlunosAsync(true)).ReturnsAsync(alunos);

            var service = new AlunoService(repository.Object);
            var result = await service.GetAllAsync(true);

            Assert.Single(result);
            Assert.Equal("Maria", result[0].Nome);
            Assert.Equal("Vinicius", result[0].Professor?.Nome);
        }

        [Fact]
        public async Task AddAsync_CreatesAlunoAndReturnsViewModel()
        {
            var viewModel = new AlunoCreateEditViewModel
            {
                Nome = "Joao",
                Sobrenome = "Gomes",
                DataNasc = "01/01/2000",
                ProfessorId = 2,
                IdUsuario = 13
            };

            var repository = new Mock<IRepository>();
            repository.Setup(r => r.ProfessorExistsAsync(viewModel.ProfessorId)).ReturnsAsync(true);
            repository.Setup(r => r.UsuarioExistsWithPerfilAsync(viewModel.IdUsuario, 3)).ReturnsAsync(true);
            repository.Setup(r => r.AlunoUsuarioInUseAsync(viewModel.IdUsuario, null)).ReturnsAsync(false);
            repository.Setup(r => r.Add(It.IsAny<Aluno>()));
            repository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
            repository.Setup(r => r.GetAlunoAsyncById(It.IsAny<int>(), true))
                .ReturnsAsync((int id, bool includeProfessor) => new Aluno
                {
                    Id = 10,
                    Nome = viewModel.Nome,
                    Sobrenome = viewModel.Sobrenome,
                    DataNasc = viewModel.DataNasc,
                    ProfessorId = viewModel.ProfessorId,
                    IdUsuario = viewModel.IdUsuario,
                    Professor = new Professor { Id = viewModel.ProfessorId, Nome = "Paula" }
                });

            var service = new AlunoService(repository.Object);
            var result = await service.AddAsync(viewModel);

            Assert.Equal(10, result.Id);
            Assert.Equal("Joao", result.Nome);
            Assert.Equal("Paula", result.Professor?.Nome);
        }
    }
}

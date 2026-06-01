using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Tests.Services
{
    public class AlunoTurmaEnsinoServiceTests
    {
        [Fact]
        public async Task GetAllAsync_WhenAdminConsultsSeed_ReturnsFiveAlunosPerTurma()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new AlunoTurmaEnsinoService(context);

            var matriculas = await service.GetAllAsync(CreatePrincipal(1, PerfilSistema.Administrador));
            var turmasSeed = new[] { 101, 102, 103, 104, 105, 106, 107, 108, 109, 201, 202, 203 };

            foreach (var turmaId in turmasSeed)
            {
                Assert.Equal(5, matriculas.Count(matricula => matricula.IdTurmaEnsino == turmaId));
            }
        }

        [Fact]
        public async Task AddAsync_WhenAdminMatriculatesAluno_CreatesMatricula()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();
            await CriarAlunoSemMatriculaAsync(context, 900);

            var service = new AlunoTurmaEnsinoService(context);

            var created = await service.AddAsync(new AlunoTurmaEnsinoCreateUpdateViewModel
            {
                IdAlunoUsuario = 900,
                IdTurmaEnsino = 106
            }, CreatePrincipal(1, PerfilSistema.Administrador));

            Assert.True(created.IdAlunoTurmaEnsino > 0);
            Assert.Equal(900, created.IdAlunoUsuario);
            Assert.Equal("Aluno Teste Matricula", created.NomeAluno);
            Assert.Equal(1, created.IdTipoEnsino);
            Assert.Equal("Ensino Fundamental", created.NomeTipoEnsino);
            Assert.Equal(106, created.IdTurmaEnsino);
            Assert.Equal("6º ano", created.NomeTurmaEnsino);
            Assert.Equal("EF6", created.CodigoTurma);
            Assert.Equal(1, created.IdUsuarioResponsavel);
            Assert.Equal("Administrador Sistema", created.NomeUsuarioResponsavel);
        }

        [Fact]
        public async Task AddAsync_WhenAlunoAlreadyHasMatricula_ThrowsInvalidOperationException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new AlunoTurmaEnsinoService(context);
            var principal = CreatePrincipal(1, PerfilSistema.Administrador);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddAsync(new AlunoTurmaEnsinoCreateUpdateViewModel
                {
                    IdAlunoUsuario = 12,
                    IdTurmaEnsino = 107
                }, principal));

            Assert.Equal("Este aluno ja esta matriculado em uma turma.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenAdminChangesTurma_UpdatesMatricula()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new AlunoTurmaEnsinoService(context);
            var principal = CreatePrincipal(1, PerfilSistema.Administrador);
            var matricula = await context.AlunosTurmasEnsino
                .SingleAsync(item => item.IdAlunoUsuario == 12);

            var updated = await service.UpdateAsync(matricula.IdAlunoTurmaEnsino, new AlunoTurmaEnsinoCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdTurmaEnsino = 107
            }, principal);

            Assert.NotNull(updated);
            Assert.Equal(107, updated!.IdTurmaEnsino);
            Assert.Equal("7º ano", updated.NomeTurmaEnsino);
            Assert.Equal("EF7", updated.CodigoTurma);
        }

        [Fact]
        public async Task AddAsync_WhenProfessorTriesToMatriculate_ThrowsUnauthorizedAccessException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new AlunoTurmaEnsinoService(context);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AddAsync(new AlunoTurmaEnsinoCreateUpdateViewModel
                {
                    IdAlunoUsuario = 12,
                    IdTurmaEnsino = 106
                }, CreatePrincipal(2, PerfilSistema.Professor)));
        }

        private static ClaimsPrincipal CreatePrincipal(int usuarioId, string perfil)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new Claim(ClaimTypes.Role, perfil)
            }, "Test");

            return new ClaimsPrincipal(identity);
        }

        private static async Task CriarAlunoSemMatriculaAsync(DataContext context, int idUsuario)
        {
            context.Usuarios.Add(new Usuario
            {
                IdUsuario = idUsuario,
                Nome = "Aluno Teste Matricula",
                Email = $"aluno.teste.{idUsuario}@escola.com",
                Telefone = "11900000000",
                Senha = "Senha@123",
                IdPerfil = PerfilSistema.AlunoId
            });

            context.Alunos.Add(new Aluno
            {
                Id = idUsuario,
                Nome = "Aluno",
                Sobrenome = "Teste Matricula",
                DataNasc = "01/01/2010",
                ProfessorId = 1,
                IdUsuario = idUsuario
            });

            await context.SaveChangesAsync();
        }

        private static DataContext CreateContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;

            return new DataContext(options);
        }
    }
}

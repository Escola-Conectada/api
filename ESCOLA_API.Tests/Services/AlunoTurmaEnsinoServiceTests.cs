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
        public async Task AddAsync_WhenAdminMatriculatesAluno_CreatesMatricula()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new AlunoTurmaEnsinoService(context);

            var created = await service.AddAsync(new AlunoTurmaEnsinoCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdTurmaEnsino = 106
            }, CreatePrincipal(1, PerfilSistema.Administrador));

            Assert.True(created.IdAlunoTurmaEnsino > 0);
            Assert.Equal(12, created.IdAlunoUsuario);
            Assert.Equal("Aluno Maria", created.NomeAluno);
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

            await service.AddAsync(new AlunoTurmaEnsinoCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdTurmaEnsino = 106
            }, principal);

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
            var created = await service.AddAsync(new AlunoTurmaEnsinoCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdTurmaEnsino = 106
            }, principal);

            var updated = await service.UpdateAsync(created.IdAlunoTurmaEnsino, new AlunoTurmaEnsinoCreateUpdateViewModel
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

        private static DataContext CreateContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;

            return new DataContext(options);
        }
    }
}

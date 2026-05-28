using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Tests.Services
{
    public class DisciplinaEventoServiceTests
    {
        [Fact]
        public async Task AddAsync_WhenProfessorOwnsDisciplina_CreatesEvento()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var disciplina = await CriarDisciplinaAsync(context, 2);
            var service = new DisciplinaEventoService(context);
            var model = new DisciplinaEventoCreateUpdateViewModel
            {
                Tipo = TipoEventoDisciplina.Avaliacao,
                Titulo = "Prova bimestral",
                Descricao = "Conteudo dos capitulos 1 e 2",
                Data = new DateOnly(2026, 6, 10)
            };

            var created = await service.AddAsync(disciplina.IdDisciplina, model, CreatePrincipal(2, PerfilSistema.Professor));

            Assert.True(created.IdEventoDisciplina > 0);
            Assert.Equal(disciplina.IdDisciplina, created.IdDisciplina);
            Assert.Equal(TipoEventoDisciplina.Avaliacao, created.Tipo);
            Assert.Equal("Prova bimestral", created.Titulo);
            Assert.Equal(new DateOnly(2026, 6, 10), created.Data);
            Assert.Equal("Professor Vinicius", created.NomeProfessor);
        }

        [Fact]
        public async Task GetEventosAsync_WhenAlunoIsAssociatedToDisciplina_ReturnsOnlyRelatedEvents()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var disciplinaDoAluno = await CriarDisciplinaAsync(context, 2, "Matematica");
            var disciplinaDeOutroAluno = await CriarDisciplinaAsync(context, 3, "Portugues");
            context.CadernetasDigitais.Add(new CadernetaDigital
            {
                IdAlunoUsuario = 12,
                IdDisciplina = disciplinaDoAluno.IdDisciplina,
                Notas = "8",
                Presencas = 10
            });
            context.DisciplinaEventos.AddRange(
                CriarEvento(disciplinaDoAluno.IdDisciplina, "Prova", new DateOnly(2026, 6, 10)),
                CriarEvento(disciplinaDeOutroAluno.IdDisciplina, "Trabalho", new DateOnly(2026, 6, 11)));
            await context.SaveChangesAsync();

            var service = new DisciplinaEventoService(context);

            var eventos = await service.GetEventosAsync(CreatePrincipal(12, PerfilSistema.Aluno), null, 2026, 6);

            Assert.Single(eventos);
            Assert.Equal("Prova", eventos[0].Titulo);
            Assert.Equal(disciplinaDoAluno.IdDisciplina, eventos[0].IdDisciplina);
        }

        [Fact]
        public async Task AddAsync_WhenProfessorDoesNotOwnDisciplina_ThrowsInvalidOperationException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var disciplina = await CriarDisciplinaAsync(context, 3);
            var service = new DisciplinaEventoService(context);
            var model = new DisciplinaEventoCreateUpdateViewModel
            {
                Tipo = TipoEventoDisciplina.Trabalho,
                Titulo = "Entrega de trabalho",
                Data = new DateOnly(2026, 6, 12)
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddAsync(disciplina.IdDisciplina, model, CreatePrincipal(2, PerfilSistema.Professor)));

            Assert.Equal("Disciplina nao encontrada para este professor.", exception.Message);
        }

        private static DisciplinaEvento CriarEvento(int idDisciplina, string titulo, DateOnly data)
        {
            return new DisciplinaEvento
            {
                IdDisciplina = idDisciplina,
                Tipo = TipoEventoDisciplina.Avaliacao,
                Titulo = titulo,
                Data = data
            };
        }

        private static async Task<Disciplina> CriarDisciplinaAsync(DataContext context, int idProfessorUsuario, string nome = "Matematica")
        {
            var disciplina = new Disciplina
            {
                Nome = nome,
                IdProfessorUsuario = idProfessorUsuario
            };

            context.Disciplinas.Add(disciplina);
            await context.SaveChangesAsync();

            return disciplina;
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

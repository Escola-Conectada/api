using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Security;
using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Tests.Services
{
    public class CadernetaDigitalServiceTests
    {
        [Fact]
        public async Task AddAsync_WhenProfessorAssociatesAlunoToDisciplina_CreatesCaderneta()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);
            var disciplina = await service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
            {
                Nome = "Matematica"
            }, professor);

            var created = await service.AddAsync(new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdDisciplina = disciplina.IdDisciplina,
                Notas = new[] { 8.5m, 9m },
                Presencas = 18,
                Faltas = 2
            }, professor);

            Assert.Equal("Aluno Maria", created.NomeAluno);
            Assert.Equal("Matematica", created.NomeDisciplina);
            Assert.Equal(new[] { 8.5m, 9m }, created.Notas);
            Assert.Equal(8.75m, created.MediaAritmetica);
            Assert.Equal("Aprovado", created.Situacao);
            Assert.Equal("azul", created.CorSituacao);
            Assert.Equal(18, created.Presencas);
            Assert.Equal(2, created.Faltas);
            var notificacao = await context.Notificacoes.SingleAsync(item => item.IdUsuario == created.IdAlunoUsuario);
            Assert.Equal("NotasPublicadas", notificacao.Tipo);
            Assert.Equal("Notas publicadas", notificacao.Titulo);
            Assert.Contains("Matematica", notificacao.Mensagem);
            Assert.Contains("Media: 8,75", notificacao.Mensagem);
            Assert.Contains("Presencas: 18", notificacao.Mensagem);
            Assert.Contains("Faltas: 2", notificacao.Mensagem);
            Assert.Equal(created.IdCadernetaDigital, notificacao.IdCadernetaDigital);
        }

        [Fact]
        public async Task UpdateAsync_WhenProfessorUpdatesCaderneta_CreatesNotificationForAluno()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);
            var disciplina = await service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
            {
                Nome = "Matematica"
            }, professor);

            var created = await service.AddAsync(new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdDisciplina = disciplina.IdDisciplina,
                Notas = new[] { 8m, 9m },
                Presencas = 20,
                Faltas = 1
            }, professor);

            var updated = await service.UpdateAsync(created.IdCadernetaDigital, new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdDisciplina = disciplina.IdDisciplina,
                Notas = new[] { 6m, 7m },
                Presencas = 21,
                Faltas = 2
            }, professor);

            Assert.NotNull(updated);
            var notificacoes = await context.Notificacoes
                .Where(item => item.IdUsuario == 12)
                .OrderBy(item => item.IdNotificacao)
                .ToArrayAsync();
            Assert.Equal(2, notificacoes.Length);
            Assert.Equal("Notas atualizadas", notificacoes[1].Titulo);
            Assert.Contains("Em recuperacao", notificacoes[1].Mensagem);
            Assert.Equal(updated!.IdCadernetaDigital, notificacoes[1].IdCadernetaDigital);
        }

        [Fact]
        public async Task AddAsync_WhenMediaIsLowerThanSix_ReturnsReprovado()
        {
            var created = await CriarCadernetaAsync(new[] { 5m, 6m }, 2);

            Assert.Equal(5.5m, created.MediaAritmetica);
            Assert.Equal("Reprovado", created.Situacao);
            Assert.Equal("vermelho", created.CorSituacao);
        }

        [Fact]
        public async Task AddAsync_WhenMediaIsBetweenSixAndSeven_ReturnsRecuperacao()
        {
            var created = await CriarCadernetaAsync(new[] { 6m, 7m }, 2);

            Assert.Equal(6.5m, created.MediaAritmetica);
            Assert.Equal("Em recuperacao", created.Situacao);
            Assert.Equal("preto", created.CorSituacao);
        }

        [Fact]
        public async Task AddAsync_WhenMediaIsGreaterThanSeven_ReturnsAprovado()
        {
            var created = await CriarCadernetaAsync(new[] { 7m, 8m }, 2);

            Assert.Equal(7.5m, created.MediaAritmetica);
            Assert.Equal("Aprovado", created.Situacao);
            Assert.Equal("azul", created.CorSituacao);
        }

        [Fact]
        public async Task AddAsync_WhenFaltasReachTen_ReturnsReprovadoPorFaltas()
        {
            var created = await CriarCadernetaAsync(new[] { 10m, 10m }, 10);

            Assert.Equal(10m, created.MediaAritmetica);
            Assert.Equal("Reprovado por Faltas", created.Situacao);
            Assert.Equal("vermelho", created.CorSituacao);
        }

        [Fact]
        public async Task GetAllAsync_WhenAlunoIsAssociated_ReturnsOnlyOwnCadernetas()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);
            var disciplina = await service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
            {
                Nome = "Portugues"
            }, professor);

            await service.AddAsync(new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdDisciplina = disciplina.IdDisciplina,
                Notas = new[] { 7m },
                Presencas = 10,
                Faltas = 1
            }, professor);

            var cadernetasDoAluno = await service.GetAllAsync(CreatePrincipal(12, PerfilSistema.Aluno));
            var cadernetasDeOutroAluno = await service.GetAllAsync(CreatePrincipal(13, PerfilSistema.Aluno));

            Assert.Single(cadernetasDoAluno);
            Assert.Empty(cadernetasDeOutroAluno);
        }

        [Fact]
        public async Task AddDisciplinaAsync_WhenNameDiffersOnlyByCase_ThrowsInvalidOperationException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);

            await service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
            {
                Nome = "Matematica"
            }, professor);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
                {
                    Nome = "matematica"
                }, professor));

            Assert.Equal("Disciplina ja cadastrada.", exception.Message);
        }

        [Fact]
        public async Task AddDisciplinaAsync_WhenProfessorTokenDoesNotMatchUsuario_ThrowsInvalidSessionException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);

            var exception = await Assert.ThrowsAsync<InvalidSessionException>(() =>
                service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
                {
                    Nome = "Geografia"
                }, CreatePrincipal(999, PerfilSistema.Professor)));

            Assert.Equal("Sessao invalida. Saia e entre novamente.", exception.Message);
        }

        [Fact]
        public async Task AddAsync_WhenAlunoHasDifferentDisciplinas_AllowsMultipleAssociations()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);
            var matematica = await service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
            {
                Nome = "Matematica"
            }, professor);
            var portugues = await service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
            {
                Nome = "Portugues"
            }, professor);

            await service.AddAsync(new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdDisciplina = matematica.IdDisciplina,
                Notas = new[] { 8m },
                Presencas = 10,
                Faltas = 1
            }, professor);
            await service.AddAsync(new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdDisciplina = portugues.IdDisciplina,
                Notas = new[] { 9m },
                Presencas = 12,
                Faltas = 0
            }, professor);

            var cadernetasDoAluno = await service.GetAllAsync(CreatePrincipal(12, PerfilSistema.Aluno));

            Assert.Equal(2, cadernetasDoAluno.Length);
            Assert.Contains(cadernetasDoAluno, caderneta => caderneta.NomeDisciplina == "Matematica");
            Assert.Contains(cadernetasDoAluno, caderneta => caderneta.NomeDisciplina == "Portugues");
        }

        [Fact]
        public async Task AddAsync_WhenAlunoIsAlreadyAssociatedToDisciplina_ThrowsInvalidOperationException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);
            var disciplina = await service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
            {
                Nome = "Ciencias"
            }, professor);
            var payload = new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdDisciplina = disciplina.IdDisciplina,
                Notas = new[] { 8m },
                Presencas = 10,
                Faltas = 1
            };

            await service.AddAsync(payload, professor);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddAsync(payload, professor));

            Assert.Equal("Este aluno ja esta associado a esta disciplina.", exception.Message);
        }

        [Fact]
        public async Task AddAsync_WhenAdminTriesToCreate_ThrowsUnauthorizedAccessException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
                {
                    Nome = "Historia"
                }, CreatePrincipal(1, PerfilSistema.Administrador)));
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

        private static async Task<CadernetaDigitalViewModel> CriarCadernetaAsync(decimal[] notas, int faltas)
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);
            var disciplina = await service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
            {
                Nome = $"Disciplina {Guid.NewGuid():N}"
            }, professor);

            return await service.AddAsync(new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdDisciplina = disciplina.IdDisciplina,
                Notas = notas,
                Presencas = 20,
                Faltas = faltas
            }, professor);
        }

    }
}

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
            var disciplina = await service.AddDisciplinaAsync(CriarDisciplinaPayload("Matematica"), professor);
            await MatricularAlunoAsync(context, 12, 106);

            var created = await service.AddAsync(
                CriarLancamentoPayload(12, disciplina.IdDisciplina, new[] { 8.5m, 9m }, 18, 2),
                professor);

            Assert.Equal("Aluno Maria", created.NomeAluno);
            Assert.Equal("Matematica", created.NomeDisciplina);
            Assert.Equal(1, created.IdTipoEnsino);
            Assert.Equal(106, created.IdTurmaEnsino);
            Assert.Equal(2, created.IdProfessorUsuario);
            Assert.Equal("Ensino Fundamental", created.NomeTipoEnsino);
            Assert.Equal("6º ano", created.NomeTurmaEnsino);
            Assert.Equal("Professor Vinicius", created.NomeProfessor);
            Assert.Equal(new[] { 8.5m, 9m }, created.Notas);
            Assert.Equal(8.75m, created.MediaAritmetica);
            Assert.Equal("Aprovado", created.Situacao);
            Assert.Equal("azul", created.CorSituacao);
            Assert.Equal(18, created.Presencas);
            Assert.Equal(2, created.Faltas);
            var notificacao = await context.Notificacoes.SingleAsync(item => item.IdUsuario == created.IdAlunoUsuario);
            Assert.Equal("NotasPublicadas", notificacao.Tipo);
            Assert.Equal("Notas publicadas em Matematica", notificacao.Titulo);
            Assert.Contains("Matematica", notificacao.Mensagem);
            Assert.Contains("Ensino Fundamental", notificacao.Mensagem);
            Assert.Contains("6", notificacao.Mensagem);
            Assert.Contains("Notas: 8,5 / 9", notificacao.Mensagem);
            Assert.Contains("Media: 8,75", notificacao.Mensagem);
            Assert.Contains("Situacao: Aprovado", notificacao.Mensagem);
            Assert.Contains("Presencas: 18", notificacao.Mensagem);
            Assert.Contains("Faltas: 2", notificacao.Mensagem);
            Assert.Equal("8.5;9", notificacao.Notas);
            Assert.Equal(1, notificacao.IdTipoEnsino);
            Assert.Equal("Ensino Fundamental", notificacao.NomeTipoEnsino);
            Assert.Equal(106, notificacao.IdTurmaEnsino);
            Assert.Equal("6º ano", notificacao.NomeTurmaEnsino);
            Assert.Equal(disciplina.IdDisciplina, notificacao.IdDisciplina);
            Assert.Equal("Matematica", notificacao.NomeDisciplina);
            Assert.Equal(8.75m, notificacao.MediaAritmetica);
            Assert.Equal("Aprovado", notificacao.Situacao);
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
            var disciplina = await service.AddDisciplinaAsync(CriarDisciplinaPayload("Matematica"), professor);
            await MatricularAlunoAsync(context, 12, 106);

            var created = await service.AddAsync(
                CriarLancamentoPayload(12, disciplina.IdDisciplina, new[] { 8m, 9m }, 20, 1),
                professor);

            var updated = await service.UpdateAsync(
                created.IdCadernetaDigital,
                CriarLancamentoPayload(12, disciplina.IdDisciplina, new[] { 6m, 7m }, 21, 2),
                professor);

            Assert.NotNull(updated);
            var notificacoes = await context.Notificacoes
                .Where(item => item.IdUsuario == 12)
                .OrderBy(item => item.IdNotificacao)
                .ToArrayAsync();
            Assert.Equal(2, notificacoes.Length);
            Assert.Equal("Notas atualizadas em Matematica", notificacoes[1].Titulo);
            Assert.Equal("6;7", notificacoes[1].Notas);
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
        public async Task GetAllAsync_WhenAlunoTriesToConsultCaderneta_ThrowsUnauthorizedAccessException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);
            var disciplina = await service.AddDisciplinaAsync(CriarDisciplinaPayload("Portugues"), professor);
            await MatricularAlunoAsync(context, 12, 106);

            await service.AddAsync(
                CriarLancamentoPayload(12, disciplina.IdDisciplina, new[] { 7m }, 10, 1),
                professor);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.GetAllAsync(CreatePrincipal(12, PerfilSistema.Aluno)));

            Assert.Equal("Aluno deve consultar o boletim digital.", exception.Message);
        }

        [Fact]
        public async Task AddAsync_WhenUsingCatalogDisciplina_CreatesCadernetaWithProfessorLancamento()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);
            await MatricularAlunoAsync(context, 12, 106);

            var created = await service.AddAsync(new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdTipoEnsino = 1,
                IdTurmaEnsino = 106,
                IdDisciplina = 1045,
                Notas = new[] { 8m, 9m },
                Presencas = 18,
                Faltas = 1
            }, professor);

            Assert.Equal("Matemática", created.NomeDisciplina);
            Assert.Equal(1, created.IdTipoEnsino);
            Assert.Equal("Ensino Fundamental", created.NomeTipoEnsino);
            Assert.Equal(106, created.IdTurmaEnsino);
            Assert.Equal("6º ano", created.NomeTurmaEnsino);
            Assert.Equal("Matemática", created.NomeAreaConhecimento);
            Assert.Equal(2, created.IdProfessorUsuario);
            Assert.Equal("Professor Vinicius", created.NomeProfessor);
        }

        [Fact]
        public async Task AddAsync_WhenDisciplinaDoesNotBelongToTipoTurma_ThrowsInvalidOperationException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddAsync(new CadernetaDigitalCreateUpdateViewModel
                {
                    IdAlunoUsuario = 12,
                    IdTipoEnsino = 2,
                    IdTurmaEnsino = 201,
                    IdDisciplina = 1045,
                    Notas = new[] { 8m },
                    Presencas = 10,
                    Faltas = 1
                }, CreatePrincipal(2, PerfilSistema.Professor)));

            Assert.Equal("Disciplina nao encontrada para o tipo de ensino e turma informados.", exception.Message);
        }

        [Fact]
        public async Task AddAsync_WhenAlunoIsNotMatriculadoInTurma_ThrowsInvalidOperationException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);
            var disciplina = await service.AddDisciplinaAsync(CriarDisciplinaPayload("Matematica"), professor);
            await MatricularAlunoAsync(context, 12, 107);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddAsync(
                    CriarLancamentoPayload(12, disciplina.IdDisciplina, new[] { 8m }, 10, 1),
                    professor));

            Assert.Equal("Aluno nao matriculado na turma informada.", exception.Message);
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
        public async Task GetEstruturaEnsinoAsync_ReturnsSeededCurriculum()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);

            var estrutura = await service.GetEstruturaEnsinoAsync(CreatePrincipal(1, PerfilSistema.Administrador));

            Assert.Equal(2, estrutura.Length);

            var fundamental = Assert.Single(estrutura, tipo => tipo.Nome == "Ensino Fundamental");
            Assert.Equal(9, fundamental.Turmas.Length);

            var primeiroAno = Assert.Single(fundamental.Turmas, turma => turma.Codigo == "EF1");
            Assert.DoesNotContain(
                primeiroAno.AreasConhecimento.SelectMany(area => area.Disciplinas),
                disciplina => disciplina.Nome == "Língua Inglesa");

            var sextoAno = Assert.Single(fundamental.Turmas, turma => turma.Codigo == "EF6");
            Assert.Contains(
                sextoAno.AreasConhecimento.SelectMany(area => area.Disciplinas),
                disciplina => disciplina.Nome == "Língua Inglesa");

            var ensinoReligioso = Assert.Single(
                sextoAno.AreasConhecimento.SelectMany(area => area.Disciplinas),
                disciplina => disciplina.Nome == "Ensino Religioso");
            Assert.True(ensinoReligioso.OfertaObrigatoria);
            Assert.True(ensinoReligioso.MatriculaFacultativa);

            var medio = Assert.Single(estrutura, tipo => tipo.Nome == "Ensino Médio");
            Assert.Equal(3, medio.Turmas.Length);

            var primeiraSerie = Assert.Single(medio.Turmas, turma => turma.Codigo == "EM1");
            var disciplinasMedio = primeiraSerie.AreasConhecimento.SelectMany(area => area.Disciplinas).ToArray();
            Assert.Contains(disciplinasMedio, disciplina => disciplina.Nome == "Física");
            Assert.Contains(disciplinasMedio, disciplina => disciplina.Nome == "Química");
            Assert.Contains(disciplinasMedio, disciplina => disciplina.Nome == "Sociologia");
        }

        [Fact]
        public async Task AddDisciplinaAsync_WhenTurmaAndAreaBelongToDifferentTipos_ThrowsInvalidOperationException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new CadernetaDigitalService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddDisciplinaAsync(new DisciplinaCreateUpdateViewModel
                {
                    Nome = "Projeto Integrador",
                    IdTurmaEnsino = 101,
                    IdAreaConhecimento = 201
                }, CreatePrincipal(2, PerfilSistema.Professor)));

            Assert.Equal("A area de conhecimento deve pertencer ao mesmo tipo de ensino da turma.", exception.Message);
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
            var matematica = await service.AddDisciplinaAsync(CriarDisciplinaPayload("Matematica"), professor);
            var portugues = await service.AddDisciplinaAsync(CriarDisciplinaPayload("Portugues"), professor);
            await MatricularAlunoAsync(context, 12, 106);

            await service.AddAsync(
                CriarLancamentoPayload(12, matematica.IdDisciplina, new[] { 8m }, 10, 1),
                professor);
            await service.AddAsync(
                CriarLancamentoPayload(12, portugues.IdDisciplina, new[] { 9m }, 12, 0),
                professor);

            var cadernetasDoAluno = await service.GetAllAsync(professor);

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
            var disciplina = await service.AddDisciplinaAsync(CriarDisciplinaPayload("Ciencias"), professor);
            await MatricularAlunoAsync(context, 12, 106);
            var payload = CriarLancamentoPayload(12, disciplina.IdDisciplina, new[] { 8m }, 10, 1);

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

        private static DisciplinaCreateUpdateViewModel CriarDisciplinaPayload(string nome)
        {
            return new DisciplinaCreateUpdateViewModel
            {
                Nome = nome,
                IdTurmaEnsino = 106,
                IdAreaConhecimento = 102
            };
        }

        private static CadernetaDigitalCreateUpdateViewModel CriarLancamentoPayload(
            int idAlunoUsuario,
            int idDisciplina,
            decimal[] notas,
            int presencas,
            int faltas)
        {
            return new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = idAlunoUsuario,
                IdTipoEnsino = 1,
                IdTurmaEnsino = 106,
                IdDisciplina = idDisciplina,
                Notas = notas,
                Presencas = presencas,
                Faltas = faltas
            };
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
            var disciplina = await service.AddDisciplinaAsync(
                CriarDisciplinaPayload($"Disciplina {Guid.NewGuid():N}"),
                professor);
            await MatricularAlunoAsync(context, 12, 106);

            return await service.AddAsync(
                CriarLancamentoPayload(12, disciplina.IdDisciplina, notas, 20, faltas),
                professor);
        }

        private static async Task MatricularAlunoAsync(DataContext context, int idAlunoUsuario, int idTurmaEnsino)
        {
            var matriculasExistentes = await context.AlunosTurmasEnsino
                .Where(matricula => matricula.IdAlunoUsuario == idAlunoUsuario)
                .ToArrayAsync();

            context.AlunosTurmasEnsino.RemoveRange(matriculasExistentes);
            await context.SaveChangesAsync();

            context.AlunosTurmasEnsino.Add(new AlunoTurmaEnsino
            {
                IdAlunoUsuario = idAlunoUsuario,
                IdTurmaEnsino = idTurmaEnsino,
                IdUsuarioResponsavel = 1,
                MatriculadoEmUtc = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

    }
}

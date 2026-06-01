using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ESCOLA_API.Tests.Services
{
    public class BoletimDigitalServiceTests
    {
        [Fact]
        public async Task GetMeuBoletimAsync_WhenNotasArePartiallyLaunched_ReturnsAllTurmaDisciplinas()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var disciplinas = await CriarCurriculoTesteAsync(context);
            await MatricularAlunoAsync(context, 12, 900);
            var cadernetaService = new CadernetaDigitalService(context);
            await cadernetaService.AddAsync(
                CriarLancamentoPayload(disciplinas[0].IdDisciplina, new[] { 8m, 9m }, 18, 2),
                CreatePrincipal(2, PerfilSistema.Professor));

            var boletimService = new BoletimDigitalService(context, CreateConfiguration());
            var boletim = await boletimService.GetMeuBoletimAsync(CreatePrincipal(12, PerfilSistema.Aluno));

            Assert.Equal(12, boletim.IdAlunoUsuario);
            Assert.Equal(900, boletim.IdTurmaEnsino);
            Assert.Equal(BoletimDigitalStatus.EmAberto, boletim.Status);
            Assert.False(boletim.Completo);
            Assert.False(boletim.Liberado);
            Assert.False(boletim.PodeCompartilhar);
            Assert.Equal(2, boletim.TotalDisciplinas);
            Assert.Equal(1, boletim.DisciplinasLancadas);
            Assert.Equal(1, boletim.DisciplinasPendentes);

            var matematica = Assert.Single(boletim.Disciplinas, item => item.NomeDisciplina == "Matematica");
            Assert.True(matematica.Lancado);
            Assert.Equal(new[] { 8m, 9m }, matematica.Notas);
            Assert.Equal(8.5m, matematica.MediaAritmetica);
            Assert.Equal("Aprovado", matematica.Situacao);
            Assert.Equal(18, matematica.Presencas);
            Assert.Equal(2, matematica.Faltas);

            var portugues = Assert.Single(boletim.Disciplinas, item => item.NomeDisciplina == "Portugues");
            Assert.False(portugues.Lancado);
            Assert.Equal("Pendente", portugues.Situacao);
        }

        [Fact]
        public async Task SolicitarELiberarAsync_WhenBoletimIsComplete_EnablesPdfAndSharing()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var disciplinas = await CriarCurriculoTesteAsync(context);
            await MatricularAlunoAsync(context, 12, 900);
            var professor = CreatePrincipal(2, PerfilSistema.Professor);
            var administrador = CreatePrincipal(1, PerfilSistema.Administrador);
            var aluno = CreatePrincipal(12, PerfilSistema.Aluno);
            var cadernetaService = new CadernetaDigitalService(context);

            await cadernetaService.AddAsync(
                CriarLancamentoPayload(disciplinas[0].IdDisciplina, new[] { 8m, 9m }, 18, 2),
                professor);
            await cadernetaService.AddAsync(
                CriarLancamentoPayload(disciplinas[1].IdDisciplina, new[] { 7m, 8m }, 20, 1),
                professor);

            var boletimService = new BoletimDigitalService(context, CreateConfiguration());
            var consultaProfessor = await boletimService.GetBoletimAlunoAsync(12, professor);

            Assert.NotNull(consultaProfessor);
            Assert.True(consultaProfessor!.Completo);
            Assert.True(consultaProfessor.PodeSolicitarLiberacao);

            var pendente = await boletimService.SolicitarLiberacaoAsync(12, professor);

            Assert.NotNull(pendente);
            Assert.Equal(BoletimDigitalStatus.PendenteDiretoria, pendente!.Status);
            Assert.True(pendente.PendenteDiretoria);
            Assert.Equal(2, pendente.IdProfessorSolicitanteUsuario);

            var pendencias = await boletimService.GetPendentesLiberacaoAsync(administrador);
            var pendencia = Assert.Single(pendencias);
            Assert.Equal(12, pendencia.IdAlunoUsuario);
            Assert.True(pendencia.Completo);
            Assert.True(pendencia.PendenteLiberacao);
            Assert.Contains(await context.Notificacoes.ToArrayAsync(), item =>
                item.IdUsuario == 1 && item.Tipo == "BoletimPendenteLiberacao");

            var liberado = await boletimService.LiberarAsync(12, administrador);

            Assert.NotNull(liberado);
            Assert.Equal(BoletimDigitalStatus.Liberado, liberado!.Status);
            Assert.True(liberado.Liberado);
            Assert.True(liberado.PodeCompartilhar);
            Assert.Contains(await context.Notificacoes.ToArrayAsync(), item =>
                item.IdUsuario == 12 && item.Tipo == "BoletimLiberado");

            var alunoConsulta = await boletimService.GetMeuBoletimAsync(aluno);
            Assert.True(alunoConsulta.Liberado);

            var pdf = await boletimService.DownloadMeuBoletimPdfAsync(aluno);
            Assert.NotNull(pdf);
            Assert.Equal("application/pdf", pdf!.ContentType);
            Assert.True(pdf.Stream.Length > 0);

            var compartilhamento = await boletimService.CriarCompartilhamentoMeuBoletimAsync(aluno);
            Assert.NotNull(compartilhamento);
            Assert.False(string.IsNullOrWhiteSpace(compartilhamento!.Token));

            var pdfCompartilhado = await boletimService.DownloadBoletimCompartilhadoAsync(compartilhamento.Token);
            Assert.NotNull(pdfCompartilhado);
            Assert.Equal("application/pdf", pdfCompartilhado!.ContentType);
        }

        [Fact]
        public async Task DownloadMeuBoletimPdfAsync_WhenBoletimIsNotReleased_ThrowsInvalidOperationException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            await CriarCurriculoTesteAsync(context);
            await MatricularAlunoAsync(context, 12, 900);
            var service = new BoletimDigitalService(context, CreateConfiguration());

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.DownloadMeuBoletimPdfAsync(CreatePrincipal(12, PerfilSistema.Aluno)));

            Assert.Equal("Boletim precisa estar completo e liberado pela Diretoria.", exception.Message);
        }

        private static async Task<Disciplina[]> CriarCurriculoTesteAsync(DataContext context)
        {
            context.TiposEnsino.Add(new TipoEnsino
            {
                IdTipoEnsino = 900,
                Nome = "Ensino Teste",
                Ordem = 1
            });

            context.TurmasEnsino.Add(new TurmaEnsino
            {
                IdTurmaEnsino = 900,
                IdTipoEnsino = 900,
                Nome = "Turma Teste",
                Codigo = "TST",
                Ordem = 1
            });

            context.AreasConhecimento.Add(new AreaConhecimento
            {
                IdAreaConhecimento = 900,
                IdTipoEnsino = 900,
                Nome = "Base Teste",
                Ordem = 1
            });

            var disciplinas = new[]
            {
                new Disciplina
                {
                    Nome = "Matematica",
                    IdTurmaEnsino = 900,
                    IdAreaConhecimento = 900,
                    OfertaObrigatoria = true,
                    Ordem = 1
                },
                new Disciplina
                {
                    Nome = "Portugues",
                    IdTurmaEnsino = 900,
                    IdAreaConhecimento = 900,
                    OfertaObrigatoria = true,
                    Ordem = 2
                }
            };

            context.Disciplinas.AddRange(disciplinas);
            await context.SaveChangesAsync();
            return disciplinas;
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

        private static CadernetaDigitalCreateUpdateViewModel CriarLancamentoPayload(
            int idDisciplina,
            decimal[] notas,
            int presencas,
            int faltas)
        {
            return new CadernetaDigitalCreateUpdateViewModel
            {
                IdAlunoUsuario = 12,
                IdTipoEnsino = 900,
                IdTurmaEnsino = 900,
                IdDisciplina = idDisciplina,
                Notas = notas,
                Presencas = presencas,
                Faltas = faltas
            };
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

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "teste-chave-boletim-digital-1234567890"
                })
                .Build();
        }
    }
}

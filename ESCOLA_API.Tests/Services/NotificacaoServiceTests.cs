using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Tests.Services
{
    public class NotificacaoServiceTests
    {
        [Fact]
        public async Task AddAsync_WhenAdminSendsNotification_AllowsAlunoToReadAndMarkAsRead()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new NotificacaoService(context);
            var created = await service.AddAsync(new NotificacaoCreateViewModel
            {
                IdUsuario = 12,
                Titulo = "Aviso",
                Mensagem = "Mensagem de teste"
            }, CreatePrincipal(1, PerfilSistema.Administrador));

            Assert.Equal(12, created.IdUsuario);
            Assert.False(created.Lida);

            var notificacoes = await service.GetMinhasAsync(CreatePrincipal(12, PerfilSistema.Aluno));
            Assert.Single(notificacoes);
            Assert.Equal("Aviso", notificacoes[0].Titulo);

            var naoLidas = await service.CountNaoLidasAsync(CreatePrincipal(12, PerfilSistema.Aluno));
            Assert.Equal(1, naoLidas);

            var lida = await service.MarcarComoLidaAsync(created.IdNotificacao, CreatePrincipal(12, PerfilSistema.Aluno));
            Assert.NotNull(lida);
            Assert.True(lida!.Lida);
            Assert.NotNull(lida.LidaEmUtc);
        }

        [Fact]
        public async Task AddAsync_WhenProfessorSendsNotification_ThrowsUnauthorizedAccessException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new NotificacaoService(context);
            var model = new NotificacaoCreateViewModel
            {
                IdUsuario = 12,
                Titulo = "Aviso",
                Mensagem = "Mensagem de teste"
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AddAsync(model, CreatePrincipal(2, PerfilSistema.Professor)));
        }

        [Fact]
        public async Task AddParaPerfisAsync_WhenAdminSendsToAlunoAndProfessor_CreatesNotificationForEachUser()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var destinatariosEsperados = await context.Usuarios
                .CountAsync(usuario => usuario.IdPerfil == PerfilSistema.AlunoId || usuario.IdPerfil == PerfilSistema.ProfessorId);

            var service = new NotificacaoService(context);
            var envio = await service.AddParaPerfisAsync(new NotificacaoPerfisCreateViewModel
            {
                TiposUsuario = new[] { PerfilSistema.Aluno, PerfilSistema.Professor },
                Titulo = "Aviso geral",
                Mensagem = "Mensagem para alunos e professores"
            }, CreatePrincipal(1, PerfilSistema.Administrador));

            Assert.Equal(destinatariosEsperados, envio.Total);
            Assert.Equal(destinatariosEsperados, envio.Notificacoes.Length);
            Assert.All(envio.Notificacoes, notificacao => Assert.False(notificacao.Lida));

            var notificacoesPersistidas = await context.Notificacoes
                .Include(notificacao => notificacao.Usuario)
                .Where(notificacao => notificacao.Titulo == "Aviso geral")
                .ToArrayAsync();

            Assert.Equal(destinatariosEsperados, notificacoesPersistidas.Length);
            Assert.All(notificacoesPersistidas, notificacao =>
                Assert.Contains(notificacao.Usuario!.IdPerfil, new[] { PerfilSistema.AlunoId, PerfilSistema.ProfessorId }));
        }

        [Fact]
        public async Task GetMinhasAsync_WhenPaging_ReturnsOnlyRequestedPage()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            for (var i = 1; i <= 5; i++)
            {
                context.Notificacoes.Add(new Notificacao
                {
                    IdUsuario = 12,
                    Tipo = "Geral",
                    Titulo = $"Aviso {i}",
                    Mensagem = $"Mensagem {i}",
                    CriadaEmUtc = new DateTime(2026, 6, i, 12, 0, 0, DateTimeKind.Utc)
                });
            }

            await context.SaveChangesAsync();

            var service = new NotificacaoService(context);
            var notificacoes = await service.GetMinhasAsync(CreatePrincipal(12, PerfilSistema.Aluno), pagina: 2, tamanhoPagina: 2);

            Assert.Equal(2, notificacoes.Length);
            Assert.Equal(new[] { "Aviso 3", "Aviso 2" }, notificacoes.Select(notificacao => notificacao.Titulo).ToArray());
        }

        [Fact]
        public async Task AddParaPerfisAsync_WhenAdminDoesNotChooseProfiles_CreatesNotificationForAllProfiles()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var destinatariosEsperados = await context.Usuarios.CountAsync();

            var service = new NotificacaoService(context);
            var envio = await service.AddParaPerfisAsync(new NotificacaoPerfisCreateViewModel
            {
                Titulo = "Aviso para todos",
                Mensagem = "Mensagem enviada pelo administrador"
            }, CreatePrincipal(1, PerfilSistema.Administrador));

            Assert.Equal(destinatariosEsperados, envio.Total);
            Assert.Contains(envio.Notificacoes, notificacao => notificacao.IdUsuario == 1);
            Assert.Contains(envio.Notificacoes, notificacao => notificacao.IdUsuario == 2);
            Assert.Contains(envio.Notificacoes, notificacao => notificacao.IdUsuario == 12);
        }

        [Fact]
        public async Task AddParaPerfisAsync_WhenProfessorSendsNotification_ThrowsUnauthorizedAccessException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new NotificacaoService(context);
            var model = new NotificacaoPerfisCreateViewModel
            {
                IdsPerfis = new[] { PerfilSistema.AlunoId },
                Titulo = "Aviso",
                Mensagem = "Mensagem de teste"
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AddParaPerfisAsync(model, CreatePrincipal(2, PerfilSistema.Professor)));
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

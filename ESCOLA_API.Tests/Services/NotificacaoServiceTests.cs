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

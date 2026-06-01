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
    public class ConfiguracaoAplicacaoServiceTests
    {
        [Fact]
        public async Task GetAsync_WhenSeeded_ReturnsSchoolName()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = CreateService(context);
            var configuracao = await service.GetAsync();

            Assert.Equal("Escola Conectada", configuracao.NomeEscola);
        }

        [Fact]
        public async Task UpdateAsync_WhenAdmin_UpdatesSchoolName()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = CreateService(context);
            var updated = await service.UpdateAsync(new ConfiguracaoAplicacaoUpdateViewModel
            {
                NomeEscola = "  Colegio Aurora  "
            }, CreatePrincipal(1, PerfilSistema.Administrador));

            Assert.Equal("Colegio Aurora", updated.NomeEscola);
            Assert.Equal("Colegio Aurora", await service.GetNomeEscolaAsync());
        }

        [Fact]
        public async Task UpdateAsync_WhenProfessor_ThrowsUnauthorizedAccessException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = CreateService(context);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.UpdateAsync(new ConfiguracaoAplicacaoUpdateViewModel
                {
                    NomeEscola = "Colegio Aurora"
                }, CreatePrincipal(2, PerfilSistema.Professor)));
        }

        private static ConfiguracaoAplicacaoService CreateService(DataContext context)
        {
            var configuration = new ConfigurationBuilder().Build();
            return new ConfiguracaoAplicacaoService(context, configuration);
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

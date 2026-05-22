using System.Security.Claims;
using form_API.Data;
using form_API.Security;
using form_API.Services;
using form_API.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace form_API.Tests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task LoginAsync_WhenUsuarioUsesDefaultPassword_ReturnsChangePasswordFlag()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var usuario = await CreateDefaultPasswordUserAsync(context);
            var service = new AuthService(context, CreateConfiguration());

            var response = await service.LoginAsync(new LoginRequestViewModel
            {
                Email = usuario.Email,
                Senha = DefaultPasswordPolicy.DefaultPassword
            });

            Assert.NotNull(response);
            Assert.True(response!.DeveAlterarSenhaPadrao);
            Assert.False(string.IsNullOrWhiteSpace(response.Token));
        }

        [Fact]
        public async Task AlterarSenhaAsync_WhenPasswordChanges_RemovesDefaultPassword()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var usuario = await CreateDefaultPasswordUserAsync(context);
            var principal = CreatePrincipal(usuario.IdUsuario);
            var service = new AuthService(context, CreateConfiguration());

            var updated = await service.AlterarSenhaAsync(principal, new AlterarSenhaViewModel
            {
                SenhaAtual = DefaultPasswordPolicy.DefaultPassword,
                NovaSenha = "Senha@252526",
                ConfirmacaoSenha = "Senha@252526"
            });

            var stored = await context.Usuarios.FirstAsync(u => u.IdUsuario == usuario.IdUsuario);

            Assert.NotNull(updated);
            Assert.False(DefaultPasswordPolicy.UsesDefaultPassword(stored.Senha));
            Assert.True(PasswordHasher.VerifyPassword("Senha@252526", stored.Senha));
        }

        private static async Task<form_API.Models.Usuario> CreateDefaultPasswordUserAsync(DataContext context)
        {
            var usuario = new form_API.Models.Usuario
            {
                Nome = "Usuario Padrao",
                Email = "padrao@escola.com",
                Telefone = "11999990000",
                Senha = PasswordHasher.HashPassword(DefaultPasswordPolicy.DefaultPassword),
                IdPerfil = 2
            };

            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        private static ClaimsPrincipal CreatePrincipal(int usuarioId)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
            }, "Test");

            return new ClaimsPrincipal(identity);
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "TestJwtKeyForUnitTestsOnly_1234567890_Secret",
                    ["Jwt:Issuer"] = "form-api",
                    ["Jwt:Audience"] = "form-client",
                    ["Jwt:ExpirationMinutes"] = "120"
                })
                .Build();
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

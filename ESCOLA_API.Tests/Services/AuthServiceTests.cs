using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Security;
using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ESCOLA_API.Tests.Services
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

        [Fact]
        public async Task SolicitarRedefinicaoSenhaAsync_WhenEmailExists_StoresHashedToken()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var usuario = await CreateCustomPasswordUserAsync(context);
            var service = new AuthService(context, CreateConfiguration());

            var result = await service.SolicitarRedefinicaoSenhaAsync(new EsqueciSenhaViewModel
            {
                Email = " RESET@ESCOLA.COM "
            });

            var stored = await context.Usuarios.FirstAsync(u => u.IdUsuario == usuario.IdUsuario);

            Assert.True(result.UsuarioEncontrado);
            Assert.False(string.IsNullOrWhiteSpace(result.TokenRedefinicao));
            Assert.NotNull(result.ExpiraEmUtc);
            Assert.False(string.IsNullOrWhiteSpace(stored.ResetSenhaTokenHash));
            Assert.NotEqual(result.TokenRedefinicao, stored.ResetSenhaTokenHash);
            Assert.True(stored.ResetSenhaTokenExpiraEmUtc > DateTime.UtcNow);
        }

        [Fact]
        public async Task SolicitarRedefinicaoSenhaAsync_WhenEmailDoesNotExist_DoesNotReturnToken()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new AuthService(context, CreateConfiguration());

            var result = await service.SolicitarRedefinicaoSenhaAsync(new EsqueciSenhaViewModel
            {
                Email = "inexistente@escola.com"
            });

            Assert.False(result.UsuarioEncontrado);
            Assert.Null(result.TokenRedefinicao);
            Assert.Null(result.ExpiraEmUtc);
        }

        [Fact]
        public async Task RedefinirSenhaAsync_WhenTokenIsValid_UpdatesPasswordAndClearsToken()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var usuario = await CreateCustomPasswordUserAsync(context);
            var service = new AuthService(context, CreateConfiguration());
            var tokenResult = await service.SolicitarRedefinicaoSenhaAsync(new EsqueciSenhaViewModel
            {
                Email = usuario.Email
            });

            var result = await service.RedefinirSenhaAsync(new RedefinirSenhaViewModel
            {
                Email = usuario.Email,
                Token = tokenResult.TokenRedefinicao!,
                NovaSenha = "Senha@252527",
                ConfirmacaoSenha = "Senha@252527"
            });

            var stored = await context.Usuarios.FirstAsync(u => u.IdUsuario == usuario.IdUsuario);

            Assert.True(result);
            Assert.True(PasswordHasher.VerifyPassword("Senha@252527", stored.Senha));
            Assert.Null(stored.ResetSenhaTokenHash);
            Assert.Null(stored.ResetSenhaTokenCriadoEmUtc);
            Assert.Null(stored.ResetSenhaTokenExpiraEmUtc);
        }

        [Fact]
        public async Task RedefinirSenhaAsync_WhenTokenIsInvalid_ReturnsFalse()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var usuario = await CreateCustomPasswordUserAsync(context);
            var service = new AuthService(context, CreateConfiguration());
            await service.SolicitarRedefinicaoSenhaAsync(new EsqueciSenhaViewModel
            {
                Email = usuario.Email
            });

            var result = await service.RedefinirSenhaAsync(new RedefinirSenhaViewModel
            {
                Email = usuario.Email,
                Token = "token-invalido",
                NovaSenha = "Senha@252527",
                ConfirmacaoSenha = "Senha@252527"
            });

            Assert.False(result);
        }

        private static async Task<ESCOLA_API.Models.Usuario> CreateDefaultPasswordUserAsync(DataContext context)
        {
            var usuario = new ESCOLA_API.Models.Usuario
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

        private static async Task<ESCOLA_API.Models.Usuario> CreateCustomPasswordUserAsync(DataContext context)
        {
            var usuario = new ESCOLA_API.Models.Usuario
            {
                Nome = "Usuario Reset",
                Email = "reset@escola.com",
                Telefone = "11999990001",
                Senha = PasswordHasher.HashPassword("Senha@252526"),
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
                    ["Jwt:Issuer"] = "escola-api",
                    ["Jwt:Audience"] = "escola-client",
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

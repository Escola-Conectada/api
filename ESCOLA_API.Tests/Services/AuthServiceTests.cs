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
            var service = CreateService(context);

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
            var service = CreateService(context);

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
            var service = CreateService(context);

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

            var service = CreateService(context);

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
            var service = CreateService(context);
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
            var service = CreateService(context);
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

        [Fact]
        public async Task LoginGoogleAsync_WhenVerifiedEmailExists_ReturnsJwtSession()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var usuario = await CreateGoogleUserAsync(context);
            var validator = new FakeGoogleTokenValidator
            {
                Payload = new GoogleTokenPayload("GOOGLE@ESCOLA.COM", true, "Usuario Google", "google-subject")
            };
            var service = CreateService(context, CreateConfiguration(googleClientId: "client-id.apps.googleusercontent.com"), validator);

            var response = await service.LoginGoogleAsync(new GoogleLoginRequestViewModel
            {
                IdToken = " google-id-token "
            });

            Assert.NotNull(response);
            Assert.Equal(usuario.IdUsuario, response!.Usuario.IdUsuario);
            Assert.False(string.IsNullOrWhiteSpace(response.Token));
            Assert.Equal("google-id-token", validator.LastIdToken);
            Assert.Equal("client-id.apps.googleusercontent.com", validator.LastClientId);
        }

        [Fact]
        public async Task LoginGoogleAsync_WhenEmailIsNotRegistered_ReturnsNull()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var validator = new FakeGoogleTokenValidator
            {
                Payload = new GoogleTokenPayload("inexistente@escola.com", true, "Usuario Google", "google-subject")
            };
            var service = CreateService(context, CreateConfiguration(googleClientId: "client-id.apps.googleusercontent.com"), validator);

            var response = await service.LoginGoogleAsync(new GoogleLoginRequestViewModel
            {
                IdToken = "google-id-token"
            });

            Assert.Null(response);
        }

        [Fact]
        public async Task LoginGoogleAsync_WhenEmailIsNotVerified_ReturnsNull()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            await CreateGoogleUserAsync(context);
            var validator = new FakeGoogleTokenValidator
            {
                Payload = new GoogleTokenPayload("google@escola.com", false, "Usuario Google", "google-subject")
            };
            var service = CreateService(context, CreateConfiguration(googleClientId: "client-id.apps.googleusercontent.com"), validator);

            var response = await service.LoginGoogleAsync(new GoogleLoginRequestViewModel
            {
                IdToken = "google-id-token"
            });

            Assert.Null(response);
        }

        [Fact]
        public async Task LoginGoogleAsync_WhenUsuarioUsesDefaultPassword_RetiresDefaultPassword()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var usuario = await CreateDefaultPasswordUserAsync(context, email: "google@escola.com");
            var validator = new FakeGoogleTokenValidator
            {
                Payload = new GoogleTokenPayload("google@escola.com", true, "Usuario Google", "google-subject")
            };
            var service = CreateService(context, CreateConfiguration(googleClientId: "client-id.apps.googleusercontent.com"), validator);

            var response = await service.LoginGoogleAsync(new GoogleLoginRequestViewModel
            {
                IdToken = "google-id-token"
            });
            var stored = await context.Usuarios.FirstAsync(u => u.IdUsuario == usuario.IdUsuario);

            Assert.NotNull(response);
            Assert.False(response!.DeveAlterarSenhaPadrao);
            Assert.False(DefaultPasswordPolicy.UsesDefaultPassword(stored.Senha));
        }

        [Fact]
        public async Task LoginGoogleAsync_WhenClientIdIsMissing_Throws()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = CreateService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.LoginGoogleAsync(new GoogleLoginRequestViewModel
                {
                    IdToken = "google-id-token"
                }));
        }

        private static async Task<ESCOLA_API.Models.Usuario> CreateDefaultPasswordUserAsync(
            DataContext context,
            string email = "padrao@escola.com")
        {
            var usuario = new ESCOLA_API.Models.Usuario
            {
                Nome = "Usuario Padrao",
                Email = email,
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

        private static async Task<ESCOLA_API.Models.Usuario> CreateGoogleUserAsync(DataContext context)
        {
            var usuario = new ESCOLA_API.Models.Usuario
            {
                Nome = "Usuario Google",
                Email = "google@escola.com",
                Telefone = "11999990002",
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

        private static AuthService CreateService(
            DataContext context,
            IConfiguration? configuration = null,
            IGoogleTokenValidator? googleTokenValidator = null)
        {
            return new AuthService(
                context,
                configuration ?? CreateConfiguration(),
                googleTokenValidator ?? new FakeGoogleTokenValidator());
        }

        private static IConfiguration CreateConfiguration(string? googleClientId = null)
        {
            var values = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestJwtKeyForUnitTestsOnly_1234567890_Secret",
                ["Jwt:Issuer"] = "escola-api",
                ["Jwt:Audience"] = "escola-client",
                ["Jwt:ExpirationMinutes"] = "120"
            };

            if (!string.IsNullOrWhiteSpace(googleClientId))
            {
                values["GoogleAuth:ClientId"] = googleClientId;
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }

        private static DataContext CreateContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;

            return new DataContext(options);
        }

        private sealed class FakeGoogleTokenValidator : IGoogleTokenValidator
        {
            public GoogleTokenPayload? Payload { get; init; }
            public string? LastIdToken { get; private set; }
            public string? LastClientId { get; private set; }

            public Task<GoogleTokenPayload?> ValidateAsync(string idToken, string clientId)
            {
                LastIdToken = idToken;
                LastClientId = clientId;
                return Task.FromResult(Payload);
            }
        }
    }
}

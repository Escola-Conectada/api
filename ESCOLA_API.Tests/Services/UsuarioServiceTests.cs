using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Security;
using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Tests.Services
{
    public class UsuarioServiceTests
    {
        [Fact]
        public async Task AddAsync_WhenModelIsValid_CreatesUsuarioWithHashedPassword()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new UsuarioService(context);
            var model = new UsuarioCreateViewModel
            {
                Nome = "Usuario Novo",
                Email = "novo@escola.com",
                Telefone = "11999990000",
                TipoUsuario = PerfilSistema.Professor
            };

            var created = await service.AddAsync(model, CreatePrincipal(1, PerfilSistema.Administrador));
            var entity = await context.Usuarios.FirstAsync(usuario => usuario.IdUsuario == created.IdUsuario);

            Assert.Equal("Usuario Novo", created.Nome);
            Assert.Equal("novo@escola.com", created.Email);
            Assert.Equal("Professor", created.DescricaoPerfil);
            Assert.NotEqual(DefaultPasswordPolicy.DefaultPassword, entity.Senha);
            Assert.True(PasswordHasher.VerifyPassword(DefaultPasswordPolicy.DefaultPassword, entity.Senha));
        }

        [Fact]
        public async Task AddAsync_WhenProfessorCreatesAluno_CreatesUsuarioAsAluno()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new UsuarioService(context);
            var model = new UsuarioCreateViewModel
            {
                Nome = "Aluno Novo",
                Email = "aluno.novo@escola.com",
                Telefone = "11999990000",
                TipoUsuario = PerfilSistema.Aluno
            };

            var created = await service.AddAsync(model, CreatePrincipal(2, PerfilSistema.Professor));

            Assert.Equal(PerfilSistema.AlunoId, created.IdPerfil);
            Assert.Equal("Aluno", created.TipoUsuario);
        }

        [Fact]
        public async Task AddAsync_WhenProfessorCreatesProfessor_ThrowsUnauthorizedAccessException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new UsuarioService(context);
            var model = new UsuarioCreateViewModel
            {
                Nome = "Professor Novo",
                Email = "professor.novo@escola.com",
                Telefone = "11999990000",
                TipoUsuario = PerfilSistema.Professor
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AddAsync(model, CreatePrincipal(2, PerfilSistema.Professor)));
        }

        [Fact]
        public async Task AddAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new UsuarioService(context);
            var model = new UsuarioCreateViewModel
            {
                Nome = "Outro Admin",
                Email = " ADMIN@ESCOLA.COM ",
                Telefone = "11999990000",
                TipoUsuario = PerfilSistema.Administrador
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddAsync(model, CreatePrincipal(1, PerfilSistema.Administrador)));

            Assert.Equal("Email ja cadastrado.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenEmailBelongsToAnotherUsuario_ThrowsInvalidOperationException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new UsuarioService(context);
            var model = new UsuarioUpdateViewModel
            {
                Nome = "Professor Atualizado",
                Email = " ADMIN@ESCOLA.COM ",
                Telefone = "11999991111",
                TipoUsuario = PerfilSistema.Professor
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateAsync(2, model, CreatePrincipal(1, PerfilSistema.Administrador)));

            Assert.Equal("Email ja cadastrado.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenAlunoUpdatesOwnBasicData_UpdatesUsuario()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new UsuarioService(context);
            var model = new UsuarioUpdateViewModel
            {
                Nome = "Aluno Atualizado",
                Email = "aluno.atualizado@escola.com",
                Telefone = "11999992222"
            };

            var updated = await service.UpdateAsync(12, model, CreatePrincipal(12, PerfilSistema.Aluno));

            Assert.NotNull(updated);
            Assert.Equal("Aluno Atualizado", updated!.Nome);
            Assert.Equal("aluno.atualizado@escola.com", updated.Email);
            Assert.Equal(PerfilSistema.AlunoId, updated.IdPerfil);
        }

        [Fact]
        public async Task UpdateAsync_WhenAlunoChangesTipoUsuario_ThrowsUnauthorizedAccessException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new UsuarioService(context);
            var model = new UsuarioUpdateViewModel
            {
                Nome = "Aluno Atualizado",
                Email = "aluno.atualizado@escola.com",
                Telefone = "11999992222",
                TipoUsuario = PerfilSistema.Professor
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.UpdateAsync(12, model, CreatePrincipal(12, PerfilSistema.Aluno)));
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

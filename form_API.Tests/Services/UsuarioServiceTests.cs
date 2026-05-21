using System;
using System.Threading.Tasks;
using form_API.Data;
using form_API.Security;
using form_API.Services;
using form_API.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace form_API.Tests.Services
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
                IdPerfil = 2
            };

            var created = await service.AddAsync(model);
            var entity = await context.Usuarios.FirstAsync(usuario => usuario.IdUsuario == created.IdUsuario);

            Assert.Equal("Usuario Novo", created.Nome);
            Assert.Equal("novo@escola.com", created.Email);
            Assert.Equal("Contribuinte", created.DescricaoPerfil);
            Assert.NotEqual(DefaultPasswordPolicy.DefaultPassword, entity.Senha);
            Assert.True(PasswordHasher.VerifyPassword(DefaultPasswordPolicy.DefaultPassword, entity.Senha));
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
                Email = "admin@escola.com",
                Telefone = "11999990000",
                IdPerfil = 1
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(model));
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

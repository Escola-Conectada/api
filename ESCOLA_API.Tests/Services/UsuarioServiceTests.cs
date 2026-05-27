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
            Assert.Equal(1, entity.IdUsuarioCriador);
            Assert.Equal("Administrador Sistema", entity.NomeUsuarioCriador);
        }

        [Fact]
        public async Task AddAsync_WhenProfessorCreatesAluno_ThrowsUnauthorizedAccessException()
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

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AddAsync(model, CreatePrincipal(2, PerfilSistema.Professor)));
        }

        [Fact]
        public async Task AddAsync_WhenAdminCreatesUsuario_CreatesNotificationForUsuario()
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

            var created = await service.AddAsync(model, CreatePrincipal(1, PerfilSistema.Administrador));
            var notificacao = await context.Notificacoes.SingleAsync(item => item.IdUsuario == created.IdUsuario);

            Assert.Equal("CadastroUsuario", notificacao.Tipo);
            Assert.Equal("Cadastro criado", notificacao.Titulo);
            Assert.Contains("Administrador Sistema", notificacao.Mensagem);
            Assert.Contains("Aluno Novo", notificacao.Mensagem);
            Assert.Contains("aluno.novo@escola.com", notificacao.Mensagem);
            Assert.Contains("Voce pode editar seus dados", notificacao.Mensagem);
            Assert.Equal($"/usuarios/{created.IdUsuario}", notificacao.Link);
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
        public async Task UpdateAsync_WhenCreatedUserUpdatesOwnData_NotifiesCreator()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var usuario = await context.Usuarios.FirstAsync(item => item.IdUsuario == 12);
            usuario.IdUsuarioCriador = 1;
            usuario.NomeUsuarioCriador = "Administrador Sistema";
            await context.SaveChangesAsync();

            var service = new UsuarioService(context);
            var model = new UsuarioUpdateViewModel
            {
                Nome = "Aluno Corrigido",
                Email = "aluno.corrigido@escola.com",
                Telefone = "11999992222"
            };

            await service.UpdateAsync(12, model, CreatePrincipal(12, PerfilSistema.Aluno));

            var notificacao = await context.Notificacoes.SingleAsync(item => item.IdUsuario == 1);
            Assert.Equal("DadosUsuarioAtualizados", notificacao.Tipo);
            Assert.Equal("Dados do usuario atualizados", notificacao.Titulo);
            Assert.Contains("Aluno Corrigido", notificacao.Mensagem);
            Assert.Contains("aluno.corrigido@escola.com", notificacao.Mensagem);
            Assert.Equal("/usuarios/12", notificacao.Link);
        }

        [Fact]
        public async Task GetAllAsync_WhenProfessorConsultsUsers_ReturnsAlunosAndProfessoresOnly()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new UsuarioService(context);

            var usuarios = await service.GetAllAsync(CreatePrincipal(2, PerfilSistema.Professor));

            Assert.Contains(usuarios, usuario => usuario.IdPerfil == PerfilSistema.AlunoId);
            Assert.Contains(usuarios, usuario => usuario.IdPerfil == PerfilSistema.ProfessorId);
            Assert.DoesNotContain(usuarios, usuario => usuario.IdPerfil == PerfilSistema.AdministradorId);
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

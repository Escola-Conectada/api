using System;
using System.Linq;
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
                DataNascimento = new DateOnly(1990, 5, 12),
                NomeMae = "Mae Teste",
                NomePai = "Pai Teste",
                Endereco = "Rua Teste, 123",
                TipoUsuario = PerfilSistema.Professor
            };

            var created = await service.AddAsync(model, CreatePrincipal(1, PerfilSistema.Administrador));
            var entity = await context.Usuarios.FirstAsync(usuario => usuario.IdUsuario == created.IdUsuario);

            Assert.Equal("Usuario Novo", created.Nome);
            Assert.Equal("novo@escola.com", created.Email);
            Assert.Equal(new DateOnly(1990, 5, 12), created.DataNascimento);
            Assert.Equal("Mae Teste", created.NomeMae);
            Assert.Equal("Pai Teste", created.NomePai);
            Assert.Equal("Rua Teste, 123", created.Endereco);
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
                Telefone = "11999992222",
                DataNascimento = new DateOnly(2001, 10, 9),
                NomeMae = "Mae Atualizada",
                NomePai = "Pai Atualizado",
                Endereco = "Avenida Escola, 456"
            };

            var updated = await service.UpdateAsync(12, model, CreatePrincipal(12, PerfilSistema.Aluno));

            Assert.NotNull(updated);
            Assert.Equal("Aluno Atualizado", updated!.Nome);
            Assert.Equal("aluno.atualizado@escola.com", updated.Email);
            Assert.Equal(new DateOnly(2001, 10, 9), updated.DataNascimento);
            Assert.Equal("Mae Atualizada", updated.NomeMae);
            Assert.Equal("Pai Atualizado", updated.NomePai);
            Assert.Equal("Avenida Escola, 456", updated.Endereco);
            Assert.Equal(PerfilSistema.AlunoId, updated.IdPerfil);
        }

        [Fact]
        public async Task UpdateAsync_WhenUserUpdatesOwnData_NotifiesAdministrators()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            context.Usuarios.Add(new Usuario
            {
                IdUsuario = 999,
                Nome = "Administrador Auxiliar",
                Email = "admin.auxiliar@escola.com",
                Telefone = "11999990099",
                Senha = PasswordHasher.HashPassword(DefaultPasswordPolicy.DefaultPassword),
                IdPerfil = PerfilSistema.AdministradorId
            });
            await context.SaveChangesAsync();

            var service = new UsuarioService(context);
            var model = new UsuarioUpdateViewModel
            {
                Nome = "Aluno Corrigido",
                Email = "aluno.corrigido@escola.com",
                Telefone = "11999992222"
            };

            await service.UpdateAsync(12, model, CreatePrincipal(12, PerfilSistema.Aluno));

            var notificacoes = await context.Notificacoes
                .Where(item => item.Tipo == "DadosUsuarioAtualizados")
                .OrderBy(item => item.IdUsuario)
                .ToArrayAsync();

            Assert.Equal(2, notificacoes.Length);
            Assert.Equal(new[] { 1, 999 }, notificacoes.Select(item => item.IdUsuario).ToArray());

            var notificacao = notificacoes[0];
            Assert.Equal("DadosUsuarioAtualizados", notificacao.Tipo);
            Assert.Equal("Dados do usuario atualizados", notificacao.Titulo);
            Assert.Contains("alterou seus dados de perfil", notificacao.Mensagem);
            Assert.Contains("Dados anteriores", notificacao.Mensagem);
            Assert.Contains("Aluno Maria", notificacao.Mensagem);
            Assert.Contains("Aluno Corrigido", notificacao.Mensagem);
            Assert.Contains("aluno.corrigido@escola.com", notificacao.Mensagem);
            Assert.Contains("Data de nascimento", notificacao.Mensagem);
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
        public async Task SolicitarExclusaoContaAsync_WhenUsuarioAuthenticated_StoresRequestAndNotifiesAdmins()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new UsuarioService(context);

            var solicitacao = await service.SolicitarExclusaoContaAsync(
                CreatePrincipal(12, PerfilSistema.Aluno),
                new SolicitarExclusaoContaViewModel
                {
                    Confirmacao = true,
                    Motivo = "Nao vou mais usar o app"
                });

            var usuario = await context.Usuarios.FirstAsync(item => item.IdUsuario == 12);
            var notificacao = await context.Notificacoes
                .SingleAsync(item => item.Tipo == "ExclusaoContaSolicitada");

            Assert.NotNull(solicitacao);
            Assert.Equal(12, solicitacao!.IdUsuario);
            Assert.Equal("Nao vou mais usar o app", solicitacao.Motivo);
            Assert.NotNull(usuario.ExclusaoContaSolicitadaEmUtc);
            Assert.Equal("Nao vou mais usar o app", usuario.ExclusaoContaMotivo);
            Assert.Equal(PerfilSistema.AdministradorId, notificacao.IdUsuario);
            Assert.Contains("Aluno Maria", notificacao.Mensagem);
            Assert.Equal("/usuarios/exclusoes-conta", notificacao.Link);
        }

        [Fact]
        public async Task GetSolicitacoesExclusaoContaAsync_WhenAdmin_ReturnsPendingRequests()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await using var context = CreateContext(connection);
            await context.Database.EnsureCreatedAsync();

            var service = new UsuarioService(context);
            await service.SolicitarExclusaoContaPorEmailAsync(new SolicitarExclusaoContaPublicaViewModel
            {
                Email = "aluno01@escola.com",
                Motivo = "Solicitacao pelo site"
            });

            var solicitacoes = await service.GetSolicitacoesExclusaoContaAsync(
                CreatePrincipal(1, PerfilSistema.Administrador));

            var solicitacao = Assert.Single(solicitacoes);
            Assert.Equal(12, solicitacao.IdUsuario);
            Assert.Equal("Aluno", solicitacao.TipoUsuario);
            Assert.Equal("Solicitacao pelo site", solicitacao.Motivo);
            Assert.Equal("Pendente", solicitacao.Status);
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

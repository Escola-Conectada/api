using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace ESCOLA_API.Tests.Services
{
    public class UsuarioArquivoServiceTests
    {
        [Fact]
        public async Task UploadFotoAsync_WhenFileIsValid_SavesPhotoAndUpdatesUsuario()
        {
            var uploadRoot = CreateUploadRoot();

            try
            {
                await using var connection = new SqliteConnection("DataSource=:memory:");
                await connection.OpenAsync();
                await using var context = CreateContext(connection);
                await context.Database.EnsureCreatedAsync();

                var service = CreateService(context, uploadRoot);
                var foto = CreateFormFile("foto.jpg", "image/jpeg", new byte[] { 1, 2, 3 });

                var updated = await service.UploadFotoAsync(12, foto, CreatePrincipal(1, PerfilSistema.Administrador));

                Assert.NotNull(updated);
                Assert.StartsWith("/uploads/usuarios/12/foto/", updated!.FotoPerfilUrl);
                Assert.True(File.Exists(ToPhysicalPath(uploadRoot, updated.FotoPerfilUrl!)));
            }
            finally
            {
                DeleteDirectory(uploadRoot);
            }
        }

        [Fact]
        public async Task UploadCertificadoAsync_WhenProfessorAndPdfAreValid_SavesCertificate()
        {
            var uploadRoot = CreateUploadRoot();

            try
            {
                await using var connection = new SqliteConnection("DataSource=:memory:");
                await connection.OpenAsync();
                await using var context = CreateContext(connection);
                await context.Database.EnsureCreatedAsync();

                var service = CreateService(context, uploadRoot);
                var certificado = CreateFormFile("certificado.pdf", "application/pdf", new byte[] { 37, 80, 68, 70 });

                var created = await service.UploadCertificadoAsync(2, certificado, CreatePrincipal(1, PerfilSistema.Administrador));

                Assert.True(created.IdArquivo > 0);
                Assert.Equal(2, created.IdUsuario);
                Assert.Equal("Certificado", created.TipoArquivo);
                Assert.Equal("certificado.pdf", created.NomeOriginal);
                Assert.StartsWith("usuarios/2/certificados/", created.NomeBlob);
                Assert.True(File.Exists(ToPhysicalPath(uploadRoot, created.Url!)));
            }
            finally
            {
                DeleteDirectory(uploadRoot);
            }
        }

        private static IFormFile CreateFormFile(string fileName, string contentType, byte[] content)
        {
            var stream = new MemoryStream(content);
            return new FormFile(stream, 0, content.Length, "arquivo", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
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

        private static UsuarioArquivoService CreateService(DataContext context, string uploadRoot)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Uploads:RootPath", uploadRoot) })
                .Build();
            var storage = new LocalUsuarioArquivoStorage(new TestHostEnvironment(), configuration);

            return new UsuarioArquivoService(context, storage);
        }

        private static string CreateUploadRoot()
        {
            var uploadRoot = Path.Combine(Path.GetTempPath(), $"escola-api-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(uploadRoot);
            return uploadRoot;
        }

        private static string ToPhysicalPath(string uploadRoot, string url)
        {
            var relative = url["/uploads/".Length..].Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(uploadRoot, relative);
        }

        private static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }

        private sealed class TestHostEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; } = Environments.Development;
            public string ApplicationName { get; set; } = "ESCOLA_API.Tests";
            public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }
    }
}

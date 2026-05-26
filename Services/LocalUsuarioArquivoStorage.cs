using Microsoft.AspNetCore.Http;

namespace ESCOLA_API.Services
{
    public class LocalUsuarioArquivoStorage : IUsuarioArquivoStorage
    {
        private readonly string _uploadRoot;

        public LocalUsuarioArquivoStorage(IHostEnvironment environment, IConfiguration configuration)
        {
            _uploadRoot = ResolveUploadRoot(environment, configuration);
        }

        public async Task<ArquivoSalvo> SalvarAsync(int usuarioId, string categoria, IFormFile arquivo)
        {
            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
            var relativeDirectory = Path.Combine("usuarios", usuarioId.ToString(), categoria);
            var targetDirectory = Path.Combine(_uploadRoot, relativeDirectory);
            Directory.CreateDirectory(targetDirectory);

            var targetPath = Path.Combine(targetDirectory, nomeArquivo);
            await using var stream = File.Create(targetPath);
            await arquivo.CopyToAsync(stream);

            var nomeBlob = $"{relativeDirectory.Replace('\\', '/')}/{nomeArquivo}";
            return new ArquivoSalvo
            {
                NomeBlob = nomeBlob,
                Url = $"/uploads/{nomeBlob}"
            };
        }

        public Task RemoverAsync(string? nomeBlob, string? url)
        {
            var relative = !string.IsNullOrWhiteSpace(nomeBlob)
                ? nomeBlob
                : ObterCaminhoRelativo(url);

            if (string.IsNullOrWhiteSpace(relative))
            {
                return Task.CompletedTask;
            }

            var fullPath = Path.GetFullPath(Path.Combine(_uploadRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
            var fullRoot = Path.GetFullPath(_uploadRoot);

            if (fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        private static string? ObterCaminhoRelativo(string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return url["/uploads/".Length..];
        }

        private static string ResolveUploadRoot(IHostEnvironment environment, IConfiguration configuration)
        {
            var configured = configuration["Uploads:RootPath"];
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return Path.IsPathRooted(configured)
                    ? configured
                    : Path.Combine(environment.ContentRootPath, configured);
            }

            return Path.Combine(environment.ContentRootPath, "uploads");
        }
    }
}

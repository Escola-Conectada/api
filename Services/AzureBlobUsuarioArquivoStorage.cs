using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace ESCOLA_API.Services
{
    public class AzureBlobUsuarioArquivoStorage : IUsuarioArquivoStorage
    {
        private readonly BlobContainerClient _containerClient;
        private readonly string? _publicBaseUrl;

        public AzureBlobUsuarioArquivoStorage(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlob:ConnectionString"];
            var containerName = configuration["AzureBlob:ContainerName"] ?? "escola-uploads";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("AzureBlob:ConnectionString nao configurada.");
            }

            _containerClient = new BlobContainerClient(connectionString, containerName);
            _publicBaseUrl = configuration["AzureBlob:PublicBaseUrl"]?.TrimEnd('/');
        }

        public async Task<ArquivoSalvo> SalvarAsync(int usuarioId, string categoria, IFormFile arquivo)
        {
            await _containerClient.CreateIfNotExistsAsync();

            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            var nomeBlob = $"usuarios/{usuarioId}/{categoria}/{Guid.NewGuid():N}{extensao}";
            var blobClient = _containerClient.GetBlobClient(nomeBlob);

            await using var stream = arquivo.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = arquivo.ContentType
                }
            });

            return new ArquivoSalvo
            {
                NomeBlob = nomeBlob,
                Url = BuildPublicUrl(blobClient, nomeBlob)
            };
        }

        public async Task RemoverAsync(string? nomeBlob, string? url)
        {
            var blobName = !string.IsNullOrWhiteSpace(nomeBlob)
                ? nomeBlob
                : TryGetBlobNameFromUrl(url);

            if (string.IsNullOrWhiteSpace(blobName))
            {
                return;
            }

            await _containerClient.GetBlobClient(blobName).DeleteIfExistsAsync();
        }

        private string BuildPublicUrl(BlobClient blobClient, string nomeBlob)
        {
            if (!string.IsNullOrWhiteSpace(_publicBaseUrl))
            {
                return $"{_publicBaseUrl}/{Uri.EscapeDataString(nomeBlob).Replace("%2F", "/", StringComparison.OrdinalIgnoreCase)}";
            }

            return blobClient.Uri.ToString();
        }

        private string? TryGetBlobNameFromUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return null;
            }

            var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2 || !segments[0].Equals(_containerClient.Name, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return string.Join('/', segments.Skip(1));
        }
    }
}

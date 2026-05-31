using System.Text.Json;
using Azure;
using Azure.Storage.Queues;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public sealed class AzureQueueCadernetaDigitalEventPublisher : ICadernetaDigitalEventPublisher
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureQueueCadernetaDigitalEventPublisher> _logger;
        private readonly DatabaseCadernetaDigitalEventPublisher _databasePublisher;
        private QueueClient? _queueClient;

        public AzureQueueCadernetaDigitalEventPublisher(
            IConfiguration configuration,
            ILogger<AzureQueueCadernetaDigitalEventPublisher> logger,
            DatabaseCadernetaDigitalEventPublisher databasePublisher)
        {
            _configuration = configuration;
            _logger = logger;
            _databasePublisher = databasePublisher;
        }

        public async Task PublishNotasPublicadasAsync(
            CadernetaDigitalViewModel caderneta,
            string operacao,
            CancellationToken cancellationToken = default)
        {
            var payload = CadernetaDigitalNotificacaoBuilder.CreateMessage(caderneta, operacao);
            var queueClient = GetQueueClient();

            if (queueClient == null)
            {
                _logger.LogDebug(
                    "QueueStorage:ConnectionString nao configurada. Notificacao da caderneta {CadernetaId} sera gravada diretamente no banco.",
                    caderneta.IdCadernetaDigital);
                await _databasePublisher.PublishMessageAsync(payload, cancellationToken);
                return;
            }

            try
            {
                await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                await queueClient.SendMessageAsync(
                    JsonSerializer.Serialize(payload, JsonOptions),
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                    "Evento {Tipo} publicado na fila {QueueName}. Caderneta: {CadernetaId}, Aluno: {AlunoId}, Disciplina: {DisciplinaId}.",
                    payload.Tipo,
                    queueClient.Name,
                    payload.IdCadernetaDigital,
                    payload.IdAlunoUsuario,
                    payload.IdDisciplina);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Falha ao publicar evento da caderneta {CadernetaId} na fila. Notificacao sera gravada diretamente no banco.",
                    caderneta.IdCadernetaDigital);
                await _databasePublisher.PublishMessageAsync(payload, cancellationToken);
            }
        }

        private QueueClient? GetQueueClient()
        {
            var connectionString = GetQueueStorageConnectionString(_configuration);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return null;
            }

            return _queueClient ??= new QueueClient(
                connectionString,
                GetQueueName(_configuration),
                new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.Base64
                });
        }

        internal static string? GetQueueStorageConnectionString(IConfiguration configuration)
        {
            return new[]
                {
                    configuration["QueueStorage:ConnectionString"],
                    configuration["AzureQueueStorage:ConnectionString"],
                    configuration["AzureBlob:ConnectionString"],
                    configuration["AzureStorage:ConnectionString"]
                }
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }

        internal static string GetQueueName(IConfiguration configuration)
        {
            return configuration["QueueStorage:QueueName"] ?? "notificacoes";
        }
    }
}

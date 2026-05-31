using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using ESCOLA_API.Data;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public sealed class AzureQueueNotificacaoWorker : BackgroundService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AzureQueueNotificacaoWorker> _logger;
        private QueueClient? _queueClient;
        private QueueClient? _poisonQueueClient;

        public AzureQueueNotificacaoWorker(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<AzureQueueNotificacaoWorker> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_configuration.GetValue("QueueStorage:ConsumerEnabled", true))
            {
                _logger.LogInformation("Consumidor do Azure Queue Storage desabilitado por configuracao.");
                return;
            }

            var connectionString = AzureQueueCadernetaDigitalEventPublisher.GetQueueStorageConnectionString(_configuration);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogInformation("QueueStorage:ConnectionString nao configurada. Consumidor de notificacoes nao sera iniciado.");
                return;
            }

            _queueClient = new QueueClient(
                connectionString,
                AzureQueueCadernetaDigitalEventPublisher.GetQueueName(_configuration),
                new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.Base64
                });
            _poisonQueueClient = new QueueClient(
                connectionString,
                $"{_queueClient.Name}-poison",
                new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.Base64
                });

            await _queueClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);
            await _poisonQueueClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);
            _logger.LogInformation("Consumidor de notificacoes iniciado na fila {QueueName}.", _queueClient.Name);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var messages = await _queueClient.ReceiveMessagesAsync(
                        maxMessages: GetMaxMessages(),
                        visibilityTimeout: TimeSpan.FromMinutes(GetVisibilityTimeoutMinutes()),
                        cancellationToken: stoppingToken);

                    if (messages.Value.Length == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(GetPollingIntervalSeconds()), stoppingToken);
                        continue;
                    }

                    foreach (var message in messages.Value)
                    {
                        await ProcessMessageAsync(message, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Encerramento normal da aplicacao.
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha no loop do consumidor do Azure Queue Storage.");
                    await Task.Delay(TimeSpan.FromSeconds(GetPollingIntervalSeconds()), stoppingToken);
                }
            }
        }

        private async Task ProcessMessageAsync(QueueMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var payload = JsonSerializer.Deserialize<CadernetaDigitalNotificacaoMessage>(message.MessageText, JsonOptions);

                if (payload == null || !payload.Tipo.Equals("NotasPublicadas", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Mensagem ignorada na fila de notificacoes. MessageId: {MessageId}", message.MessageId);
                    await DeleteMessageAsync(message, cancellationToken);
                    return;
                }

                await using var scope = _scopeFactory.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var jaProcessada = await context.Notificacoes
                    .AnyAsync(notificacao => notificacao.OrigemMensagemId == payload.OrigemMensagemId, cancellationToken);

                if (jaProcessada)
                {
                    await DeleteMessageAsync(message, cancellationToken);
                    return;
                }

                var alunoExiste = await context.Usuarios
                    .AnyAsync(usuario => usuario.IdUsuario == payload.IdAlunoUsuario, cancellationToken);

                if (!alunoExiste)
                {
                    _logger.LogWarning(
                        "Mensagem {MessageId} ignorada porque o aluno {AlunoId} nao existe.",
                        message.MessageId,
                        payload.IdAlunoUsuario);
                    await DeleteMessageAsync(message, cancellationToken);
                    return;
                }

                context.Notificacoes.Add(CadernetaDigitalNotificacaoBuilder.CreateNotificacao(payload));
                await context.SaveChangesAsync(cancellationToken);
                await DeleteMessageAsync(message, cancellationToken);

                _logger.LogInformation(
                    "Notificacao criada para aluno {AlunoId} a partir da mensagem {OrigemMensagemId}.",
                    payload.IdAlunoUsuario,
                    payload.OrigemMensagemId);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Mensagem invalida na fila de notificacoes. MessageId: {MessageId}", message.MessageId);
                await MoveToPoisonAsync(message, cancellationToken);
            }
            catch (DbUpdateException ex) when (DatabaseCadernetaDigitalEventPublisher.IsDuplicateMessage(ex))
            {
                _logger.LogInformation("Mensagem {MessageId} ja havia gerado notificacao.", message.MessageId);
                await DeleteMessageAsync(message, cancellationToken);
            }
            catch (Exception ex) when (message.DequeueCount >= GetMaxDequeueCount())
            {
                _logger.LogError(
                    ex,
                    "Mensagem {MessageId} excedeu o limite de tentativas e sera movida para a fila poison.",
                    message.MessageId);
                await MoveToPoisonAsync(message, cancellationToken);
            }
        }

        private async Task DeleteMessageAsync(QueueMessage message, CancellationToken cancellationToken)
        {
            await _queueClient!.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);
        }

        private async Task MoveToPoisonAsync(QueueMessage message, CancellationToken cancellationToken)
        {
            await _poisonQueueClient!.SendMessageAsync(message.MessageText, cancellationToken);
            await DeleteMessageAsync(message, cancellationToken);
        }

        private int GetMaxMessages()
        {
            return Math.Clamp(_configuration.GetValue("QueueStorage:MaxMessages", 8), 1, 32);
        }

        private int GetMaxDequeueCount()
        {
            return Math.Max(1, _configuration.GetValue("QueueStorage:MaxDequeueCount", 5));
        }

        private int GetPollingIntervalSeconds()
        {
            return Math.Max(1, _configuration.GetValue("QueueStorage:PollingIntervalSeconds", 5));
        }

        private int GetVisibilityTimeoutMinutes()
        {
            return Math.Max(1, _configuration.GetValue("QueueStorage:VisibilityTimeoutMinutes", 2));
        }
    }
}

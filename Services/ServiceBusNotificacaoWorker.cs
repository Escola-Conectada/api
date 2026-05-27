using System.Globalization;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public sealed class ServiceBusNotificacaoWorker : BackgroundService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ServiceBusNotificacaoWorker> _logger;
        private ServiceBusClient? _client;
        private ServiceBusProcessor? _processor;

        public ServiceBusNotificacaoWorker(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<ServiceBusNotificacaoWorker> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionString = _configuration["ServiceBus:ConnectionString"];
            var queueName = _configuration["ServiceBus:QueueName"] ?? "notificacoes";
            var enabled = _configuration.GetValue("ServiceBus:ConsumerEnabled", true);

            if (!enabled)
            {
                _logger.LogInformation("Consumidor do Service Bus desabilitado por configuracao.");
                return;
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogInformation("ServiceBus:ConnectionString nao configurada. Consumidor de notificacoes nao sera iniciado.");
                return;
            }

            try
            {
                _client = new ServiceBusClient(connectionString);
                _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1
                });

                _processor.ProcessMessageAsync += ProcessMessageAsync;
                _processor.ProcessErrorAsync += ProcessErrorAsync;

                await _processor.StartProcessingAsync(stoppingToken);
                _logger.LogInformation("Consumidor de notificacoes iniciado na fila {QueueName}.", queueName);
            }
            catch (FormatException ex)
            {
                _logger.LogError(
                    ex,
                    "ServiceBus:ConnectionString invalida. Consumidor de notificacoes nao sera iniciado.");
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Falha ao iniciar consumidor do Service Bus. Consumidor de notificacoes nao sera iniciado.");
                return;
            }

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Encerramento normal da aplicacao.
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_processor != null)
            {
                await _processor.StopProcessingAsync(cancellationToken);
                await _processor.DisposeAsync();
            }

            if (_client != null)
            {
                await _client.DisposeAsync();
            }

            await base.StopAsync(cancellationToken);
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var payload = args.Message.Body.ToObjectFromJson<CadernetaDigitalNotificacaoMessage>(JsonOptions);

                if (payload == null || !payload.Tipo.Equals("NotasPublicadas", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Mensagem ignorada na fila de notificacoes. MessageId: {MessageId}", args.Message.MessageId);
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                await using var scope = _scopeFactory.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var jaProcessada = await context.Notificacoes
                    .AnyAsync(notificacao => notificacao.OrigemMensagemId == args.Message.MessageId, args.CancellationToken);

                if (jaProcessada)
                {
                    await args.CompleteMessageAsync(args.Message, args.CancellationToken);
                    return;
                }

                var alunoExiste = await context.Usuarios
                    .AnyAsync(usuario => usuario.IdUsuario == payload.IdAlunoUsuario, args.CancellationToken);

                if (!alunoExiste)
                {
                    _logger.LogWarning(
                        "Mensagem {MessageId} ignorada porque o aluno {AlunoId} nao existe.",
                        args.Message.MessageId,
                        payload.IdAlunoUsuario);
                    await args.CompleteMessageAsync(args.Message, args.CancellationToken);
                    return;
                }

                context.Notificacoes.Add(CriarNotificacao(payload, args.Message.MessageId));
                await context.SaveChangesAsync(args.CancellationToken);
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);

                _logger.LogInformation(
                    "Notificacao criada para aluno {AlunoId} a partir da mensagem {MessageId}.",
                    payload.IdAlunoUsuario,
                    args.Message.MessageId);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Mensagem invalida na fila de notificacoes. MessageId: {MessageId}", args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "PayloadInvalido", ex.Message);
            }
            catch (DbUpdateException ex) when (IsDuplicateMessage(ex))
            {
                _logger.LogInformation(
                    "Mensagem {MessageId} ja havia gerado notificacao.",
                    args.Message.MessageId);
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar mensagem de notificacao {MessageId}.", args.Message.MessageId);
                throw;
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(
                args.Exception,
                "Erro no consumidor do Service Bus. Fonte: {ErrorSource}, Entidade: {EntityPath}.",
                args.ErrorSource,
                args.EntityPath);
            return Task.CompletedTask;
        }

        private static Notificacao CriarNotificacao(CadernetaDigitalNotificacaoMessage payload, string messageId)
        {
            var media = payload.MediaAritmetica.ToString("0.##", CultureInfo.GetCultureInfo("pt-BR"));

            return new Notificacao
            {
                IdUsuario = payload.IdAlunoUsuario,
                Tipo = payload.Tipo,
                Titulo = "Notas publicadas",
                Mensagem = $"Suas notas de {payload.NomeDisciplina} foram publicadas. Media: {media}. Situacao: {payload.Situacao}.",
                Link = $"/caderneta-digital?cadernetaId={payload.IdCadernetaDigital}",
                IdCadernetaDigital = payload.IdCadernetaDigital,
                IdDisciplina = payload.IdDisciplina,
                NomeDisciplina = payload.NomeDisciplina,
                MediaAritmetica = payload.MediaAritmetica,
                Situacao = payload.Situacao,
                CorSituacao = payload.CorSituacao,
                OrigemMensagemId = messageId,
                CriadaEmUtc = payload.PublicadoEmUtc == default
                    ? DateTime.UtcNow
                    : payload.PublicadoEmUtc.UtcDateTime
            };
        }

        private static bool IsDuplicateMessage(DbUpdateException exception)
        {
            return exception.InnerException is Microsoft.Data.SqlClient.SqlException sqlException
                && sqlException.Errors.Cast<Microsoft.Data.SqlClient.SqlError>().Any(error => error.Number is 2601 or 2627);
        }
    }
}

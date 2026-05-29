using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public sealed class ServiceBusCadernetaDigitalEventPublisher : ICadernetaDigitalEventPublisher, IAsyncDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly ILogger<ServiceBusCadernetaDigitalEventPublisher> _logger;
        private readonly string? _connectionString;
        private readonly string _queueName;
        private ServiceBusClient? _client;
        private ServiceBusSender? _sender;

        public ServiceBusCadernetaDigitalEventPublisher(
            IConfiguration configuration,
            ILogger<ServiceBusCadernetaDigitalEventPublisher> logger)
        {
            _logger = logger;
            _connectionString = configuration["ServiceBus:ConnectionString"];
            _queueName = configuration["ServiceBus:QueueName"] ?? "notificacoes";
        }

        public async Task PublishNotasPublicadasAsync(
            CadernetaDigitalViewModel caderneta,
            string operacao,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _logger.LogDebug(
                    "ServiceBus:ConnectionString nao configurada. Evento da caderneta {CadernetaId} nao foi enviado.",
                    caderneta.IdCadernetaDigital);
                return;
            }

            try
            {
                var payload = new CadernetaDigitalNotificacaoMessage
                {
                    Operacao = operacao,
                    IdCadernetaDigital = caderneta.IdCadernetaDigital,
                    IdAlunoUsuario = caderneta.IdAlunoUsuario,
                    NomeAluno = caderneta.NomeAluno,
                    EmailAluno = caderneta.EmailAluno,
                    IdDisciplina = caderneta.IdDisciplina,
                    NomeDisciplina = caderneta.NomeDisciplina,
                    IdProfessorUsuario = caderneta.IdProfessorUsuario ?? 0,
                    NomeProfessor = caderneta.NomeProfessor,
                    Notas = caderneta.Notas,
                    MediaAritmetica = caderneta.MediaAritmetica,
                    Situacao = caderneta.Situacao,
                    CorSituacao = caderneta.CorSituacao,
                    Presencas = caderneta.Presencas,
                    Faltas = caderneta.Faltas,
                    PublicadoEmUtc = DateTimeOffset.UtcNow
                };

                var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(payload, JsonOptions))
                {
                    ContentType = "application/json",
                    Subject = payload.Tipo,
                    MessageId = $"caderneta-{payload.IdCadernetaDigital}-{payload.PublicadoEmUtc.ToUnixTimeMilliseconds()}"
                };

                message.ApplicationProperties["tipo"] = payload.Tipo;
                message.ApplicationProperties["operacao"] = payload.Operacao;
                message.ApplicationProperties["idAlunoUsuario"] = payload.IdAlunoUsuario;
                message.ApplicationProperties["idDisciplina"] = payload.IdDisciplina;

                await GetSender().SendMessageAsync(message, cancellationToken);

                _logger.LogInformation(
                    "Evento {Tipo} enviado para a fila {QueueName}. Caderneta: {CadernetaId}, Aluno: {AlunoId}, Disciplina: {DisciplinaId}.",
                    payload.Tipo,
                    _queueName,
                    payload.IdCadernetaDigital,
                    payload.IdAlunoUsuario,
                    payload.IdDisciplina);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao enviar evento da caderneta {CadernetaId} para a fila {QueueName}.",
                    caderneta.IdCadernetaDigital,
                    _queueName);
            }
        }

        private ServiceBusSender GetSender()
        {
            _client ??= new ServiceBusClient(_connectionString);
            _sender ??= _client.CreateSender(_queueName);
            return _sender;
        }

        public async ValueTask DisposeAsync()
        {
            if (_sender != null)
            {
                await _sender.DisposeAsync();
            }

            if (_client != null)
            {
                await _client.DisposeAsync();
            }
        }

    }
}

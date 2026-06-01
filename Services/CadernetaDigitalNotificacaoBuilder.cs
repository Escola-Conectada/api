using System.Globalization;
using ESCOLA_API.Models;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    internal static class CadernetaDigitalNotificacaoBuilder
    {
        public static CadernetaDigitalNotificacaoMessage CreateMessage(
            CadernetaDigitalViewModel caderneta,
            string operacao,
            string? nomeEscola = null)
        {
            var publicadoEmUtc = DateTimeOffset.UtcNow;
            var origemMensagemId = $"caderneta-{caderneta.IdCadernetaDigital}-{NormalizarOperacao(operacao)}-{publicadoEmUtc.ToUnixTimeMilliseconds()}-{Guid.NewGuid():N}";

            return new CadernetaDigitalNotificacaoMessage
            {
                Operacao = operacao,
                OrigemMensagemId = origemMensagemId,
                NomeEscola = NormalizarNomeEscola(nomeEscola),
                IdCadernetaDigital = caderneta.IdCadernetaDigital,
                IdAlunoUsuario = caderneta.IdAlunoUsuario,
                NomeAluno = caderneta.NomeAluno,
                EmailAluno = caderneta.EmailAluno,
                IdTipoEnsino = caderneta.IdTipoEnsino,
                NomeTipoEnsino = caderneta.NomeTipoEnsino,
                IdTurmaEnsino = caderneta.IdTurmaEnsino,
                NomeTurmaEnsino = caderneta.NomeTurmaEnsino,
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
                PublicadoEmUtc = publicadoEmUtc
            };
        }

        public static Notificacao CreateNotificacao(CadernetaDigitalNotificacaoMessage payload)
        {
            var notas = payload.Notas.Length == 0
                ? "-"
                : string.Join(" / ", payload.Notas.Select(FormatarDecimalPtBr));
            var media = payload.MediaAritmetica.ToString("0.##", CultureInfo.GetCultureInfo("pt-BR"));
            var contexto = FormatarContextoDisciplina(payload);
            var atualizacao = payload.Operacao.Equals("Atualizacao", StringComparison.OrdinalIgnoreCase);
            var acao = atualizacao ? "atualizadas" : "publicadas";
            var titulo = atualizacao
                ? $"Notas atualizadas em {payload.NomeDisciplina}"
                : $"Notas publicadas em {payload.NomeDisciplina}";
            var nomeEscola = NormalizarNomeEscola(payload.NomeEscola);

            return new Notificacao
            {
                IdUsuario = payload.IdAlunoUsuario,
                Tipo = payload.Tipo,
                Titulo = titulo,
                Mensagem = $"No {nomeEscola}, suas notas de {payload.NomeDisciplina}{contexto} foram {acao} pelo professor {payload.NomeProfessor}. Notas: {notas}. Media: {media}. Situacao: {payload.Situacao}. Presencas: {payload.Presencas}. Faltas: {payload.Faltas}.",
                Link = "/boletim-digital",
                IdCadernetaDigital = payload.IdCadernetaDigital,
                Notas = SerializeNotas(payload.Notas),
                IdTipoEnsino = payload.IdTipoEnsino,
                NomeTipoEnsino = payload.NomeTipoEnsino,
                IdTurmaEnsino = payload.IdTurmaEnsino,
                NomeTurmaEnsino = payload.NomeTurmaEnsino,
                IdDisciplina = payload.IdDisciplina,
                NomeDisciplina = payload.NomeDisciplina,
                MediaAritmetica = payload.MediaAritmetica,
                Situacao = payload.Situacao,
                CorSituacao = payload.CorSituacao,
                OrigemMensagemId = payload.OrigemMensagemId,
                CriadaEmUtc = payload.PublicadoEmUtc == default
                    ? DateTime.UtcNow
                    : payload.PublicadoEmUtc.UtcDateTime
            };
        }

        private static string SerializeNotas(decimal[] notas)
        {
            return string.Join(";", notas.Select(nota => nota.ToString(CultureInfo.InvariantCulture)));
        }

        private static string FormatarDecimalPtBr(decimal valor)
        {
            return valor.ToString("0.##", CultureInfo.GetCultureInfo("pt-BR"));
        }

        private static string FormatarContextoDisciplina(CadernetaDigitalNotificacaoMessage payload)
        {
            var partes = new[]
            {
                payload.NomeTipoEnsino,
                payload.NomeTurmaEnsino
            }.Where(parte => !string.IsNullOrWhiteSpace(parte));

            var contexto = string.Join(" - ", partes);
            return string.IsNullOrWhiteSpace(contexto) ? string.Empty : $" ({contexto})";
        }

        private static string NormalizarOperacao(string operacao)
        {
            return string.IsNullOrWhiteSpace(operacao)
                ? "evento"
                : operacao.Trim().ToLowerInvariant();
        }

        private static string NormalizarNomeEscola(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ConfiguracaoAplicacaoService.NomeEscolaPadrao
                : value.Trim();
        }
    }
}

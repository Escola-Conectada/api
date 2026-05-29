namespace ESCOLA_API.Services
{
    public sealed class CadernetaDigitalNotificacaoMessage
    {
        public string Tipo { get; init; } = "NotasPublicadas";
        public string Operacao { get; init; } = string.Empty;
        public int IdCadernetaDigital { get; init; }
        public int IdAlunoUsuario { get; init; }
        public string NomeAluno { get; init; } = string.Empty;
        public string EmailAluno { get; init; } = string.Empty;
        public int? IdTipoEnsino { get; init; }
        public string? NomeTipoEnsino { get; init; }
        public int? IdTurmaEnsino { get; init; }
        public string? NomeTurmaEnsino { get; init; }
        public int IdDisciplina { get; init; }
        public string NomeDisciplina { get; init; } = string.Empty;
        public int IdProfessorUsuario { get; init; }
        public string NomeProfessor { get; init; } = string.Empty;
        public decimal[] Notas { get; init; } = [];
        public decimal MediaAritmetica { get; init; }
        public string Situacao { get; init; } = string.Empty;
        public string CorSituacao { get; init; } = string.Empty;
        public int Presencas { get; init; }
        public int Faltas { get; init; }
        public DateTimeOffset PublicadoEmUtc { get; init; }
    }
}

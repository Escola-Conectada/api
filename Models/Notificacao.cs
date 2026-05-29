namespace ESCOLA_API.Models
{
    public class Notificacao
    {
        public int IdNotificacao { get; set; }
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
        public string Tipo { get; set; } = "Geral";
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public string? Link { get; set; }
        public int? IdCadernetaDigital { get; set; }
        public string? Notas { get; set; }
        public int? IdTipoEnsino { get; set; }
        public string? NomeTipoEnsino { get; set; }
        public int? IdTurmaEnsino { get; set; }
        public string? NomeTurmaEnsino { get; set; }
        public int? IdDisciplina { get; set; }
        public string? NomeDisciplina { get; set; }
        public decimal? MediaAritmetica { get; set; }
        public string? Situacao { get; set; }
        public string? CorSituacao { get; set; }
        public string? OrigemMensagemId { get; set; }
        public bool Lida { get; set; }
        public DateTime CriadaEmUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LidaEmUtc { get; set; }
    }
}

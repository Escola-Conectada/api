namespace ESCOLA_API.ViewModels
{
    public class NotificacaoViewModel
    {
        public int IdNotificacao { get; set; }
        public int IdUsuario { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public string? Link { get; set; }
        public int? IdCadernetaDigital { get; set; }
        public decimal[] Notas { get; set; } = [];
        public int? IdTipoEnsino { get; set; }
        public string? NomeTipoEnsino { get; set; }
        public int? IdTurmaEnsino { get; set; }
        public string? NomeTurmaEnsino { get; set; }
        public int? IdDisciplina { get; set; }
        public string? NomeDisciplina { get; set; }
        public decimal? MediaAritmetica { get; set; }
        public string? Situacao { get; set; }
        public string? CorSituacao { get; set; }
        public bool Lida { get; set; }
        public DateTime CriadaEmUtc { get; set; }
        public DateTime? LidaEmUtc { get; set; }
    }

    public class NotificacaoCreateViewModel
    {
        public int IdUsuario { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Geral";
        public string? Link { get; set; }
    }

    /// <summary>
    /// Payload para envio administrativo de notificacao para todos os usuarios de um ou mais perfis.
    /// </summary>
    public class NotificacaoPerfisCreateViewModel
    {
        /// <summary>
        /// Identificadores dos perfis destinatarios, como 2 para Professor e 3 para Aluno.
        /// </summary>
        public int[] IdsPerfis { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Nomes dos perfis destinatarios, como Aluno, Professor ou Administrador.
        /// </summary>
        public string[] TiposUsuario { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Quando verdadeiro, envia para todos os perfis cadastrados.
        /// </summary>
        public bool TodosOsPerfis { get; set; }

        /// <summary>
        /// Titulo da notificacao.
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem da notificacao.
        /// </summary>
        public string Mensagem { get; set; } = string.Empty;

        /// <summary>
        /// Tipo da notificacao. Usa Geral quando omitido.
        /// </summary>
        public string Tipo { get; set; } = "Geral";

        /// <summary>
        /// Link opcional para direcionar o usuario no front-end.
        /// </summary>
        public string? Link { get; set; }
    }

    /// <summary>
    /// Resultado do envio em lote de notificacoes.
    /// </summary>
    public class NotificacaoEnvioViewModel
    {
        /// <summary>
        /// Total de notificacoes criadas.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Notificacoes criadas para os destinatarios.
        /// </summary>
        public NotificacaoViewModel[] Notificacoes { get; set; } = Array.Empty<NotificacaoViewModel>();
    }
}

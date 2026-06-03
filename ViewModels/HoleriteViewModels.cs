using Microsoft.AspNetCore.Http;

namespace ESCOLA_API.ViewModels
{
    /// <summary>
    /// Metadados de um holerite em PDF vinculado a professor ou administrador.
    /// </summary>
    public class HoleriteViewModel
    {
        /// <summary>
        /// Identificador do holerite.
        /// </summary>
        public int IdHolerite { get; set; }

        /// <summary>
        /// Identificador do usuario funcionario.
        /// </summary>
        public int IdUsuario { get; set; }

        /// <summary>
        /// Nome do usuario funcionario.
        /// </summary>
        public string NomeUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Perfil do usuario funcionario.
        /// </summary>
        public string PerfilUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Mes de competencia do holerite, de 1 a 12.
        /// </summary>
        public int CompetenciaMes { get; set; }

        /// <summary>
        /// Ano de competencia do holerite.
        /// </summary>
        public int CompetenciaAno { get; set; }

        /// <summary>
        /// Competencia formatada como MM/yyyy.
        /// </summary>
        public string Competencia { get; set; } = string.Empty;

        /// <summary>
        /// Nome original do arquivo PDF enviado.
        /// </summary>
        public string NomeOriginal { get; set; } = string.Empty;

        /// <summary>
        /// URL do arquivo quando o storage expuser acesso publico.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Content-Type do arquivo, esperado como application/pdf.
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Tamanho do arquivo em bytes.
        /// </summary>
        public long TamanhoBytes { get; set; }

        /// <summary>
        /// Data e hora UTC de envio do holerite.
        /// </summary>
        public DateTime CriadoEmUtc { get; set; }
    }

    /// <summary>
    /// Dados do formulario para envio de holerite em PDF.
    /// </summary>
    public class HoleriteUploadViewModel
    {
        /// <summary>
        /// Arquivo PDF do holerite.
        /// </summary>
        public IFormFile? Arquivo { get; set; }

        /// <summary>
        /// Mes de competencia do holerite, de 1 a 12.
        /// </summary>
        public int CompetenciaMes { get; set; }

        /// <summary>
        /// Ano de competencia do holerite.
        /// </summary>
        public int CompetenciaAno { get; set; }
    }

    /// <summary>
    /// Link temporario de compartilhamento de holerite.
    /// </summary>
    public class HoleriteCompartilhamentoViewModel
    {
        /// <summary>
        /// Token assinado usado pela rota publica de download temporario.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// URL temporaria para abrir ou baixar o holerite sem cabecalho Authorization.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora UTC de expiracao do link.
        /// </summary>
        public DateTime ExpiraEmUtc { get; set; }
    }
}

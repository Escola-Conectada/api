namespace ESCOLA_API.Models
{
    public class ConfiguracaoAplicacao
    {
        public int IdConfiguracaoAplicacao { get; set; }
        public string NomeEscola { get; set; } = string.Empty;
        public DateTime AtualizadoEmUtc { get; set; }
    }
}

namespace ESCOLA_API.ViewModels
{
    public class ConfiguracaoAplicacaoViewModel
    {
        public string NomeEscola { get; set; } = string.Empty;
        public DateTime AtualizadoEmUtc { get; set; }
    }

    public class ConfiguracaoAplicacaoUpdateViewModel
    {
        public string NomeEscola { get; set; } = string.Empty;
    }
}

namespace ESCOLA_API.Models
{
    public static class TipoEventoDisciplina
    {
        public const string Avaliacao = "Avaliacao";
        public const string Trabalho = "Trabalho";

        private static readonly string[] TiposValidos = [Avaliacao, Trabalho];

        public static bool TipoValido(string? tipo)
        {
            return !string.IsNullOrWhiteSpace(tipo)
                && TiposValidos.Contains(tipo.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        public static string Normalizar(string tipo)
        {
            return tipo.Trim().Equals(Trabalho, StringComparison.OrdinalIgnoreCase)
                ? Trabalho
                : Avaliacao;
        }
    }
}

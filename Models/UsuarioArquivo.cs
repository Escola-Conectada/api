namespace ESCOLA_API.Models
{
    public class UsuarioArquivo
    {
        public int IdArquivo { get; set; }
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
        public string? NomeBlob { get; set; }
        public string? TipoArquivo { get; set; }
        public string? NomeOriginal { get; set; }
        public string? Url { get; set; }
        public string? ContentType { get; set; }
        public long? TamanhoBytes { get; set; }
        public DateTime? CriadoEmUtc { get; set; } = DateTime.UtcNow;
    }
}

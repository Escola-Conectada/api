namespace ESCOLA_API.ViewModels
{
    public class AlunoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Sobrenome { get; set; } = string.Empty;
        public string DataNasc { get; set; } = string.Empty;
        public int ProfessorId { get; set; }
        public ProfessorSummaryViewModel? Professor { get; set; }
        public int IdUsuario { get; set; }
        public UsuarioSummaryViewModel? Usuario { get; set; }
    }

    public class AlunoSummaryViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Sobrenome { get; set; } = string.Empty;
        public int ProfessorId { get; set; }
        public int IdUsuario { get; set; }
    }

    public class ProfessorSummaryViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
    }
}

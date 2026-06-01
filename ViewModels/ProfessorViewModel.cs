using System.Collections.Generic;

namespace ESCOLA_API.ViewModels
{
    public class ProfessorViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public UsuarioSummaryViewModel? Usuario { get; set; }
        public IEnumerable<AlunoSummaryViewModel> Alunos { get; set; } = new List<AlunoSummaryViewModel>();
    }
}

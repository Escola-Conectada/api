using System.Collections.Generic;

namespace ESCOLA_API.Models
{
    public class Disciplina
    {
        public int IdDisciplina { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int IdProfessorUsuario { get; set; }
        public Usuario? ProfessorUsuario { get; set; }
        public List<CadernetaDigital> Cadernetas { get; set; } = new();
        public List<DisciplinaEvento> Eventos { get; set; } = new();
    }
}

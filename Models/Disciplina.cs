using System.Collections.Generic;

namespace ESCOLA_API.Models
{
    public class Disciplina
    {
        public int IdDisciplina { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int? IdProfessorUsuario { get; set; }
        public Usuario? ProfessorUsuario { get; set; }
        public int? IdTurmaEnsino { get; set; }
        public TurmaEnsino? TurmaEnsino { get; set; }
        public int? IdAreaConhecimento { get; set; }
        public AreaConhecimento? AreaConhecimento { get; set; }
        public string? Observacao { get; set; }
        public bool OfertaObrigatoria { get; set; } = true;
        public bool MatriculaFacultativa { get; set; }
        public int Ordem { get; set; }
        public List<CadernetaDigital> Cadernetas { get; set; } = new();
        public List<DisciplinaEvento> Eventos { get; set; } = new();
    }
}

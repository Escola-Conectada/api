using System.Collections.Generic;

namespace ESCOLA_API.Models
{
    public class TurmaEnsino
    {
        public int IdTurmaEnsino { get; set; }
        public int IdTipoEnsino { get; set; }
        public TipoEnsino? TipoEnsino { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public List<Disciplina> Disciplinas { get; set; } = new();
        public List<AlunoTurmaEnsino> AlunosMatriculados { get; set; } = new();
    }
}

using System.Collections.Generic;

namespace ESCOLA_API.Models
{
    public class TipoEnsino
    {
        public int IdTipoEnsino { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public List<TurmaEnsino> Turmas { get; set; } = new();
        public List<AreaConhecimento> AreasConhecimento { get; set; } = new();
    }
}

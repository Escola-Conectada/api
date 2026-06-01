using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESCOLA_API.Models
{
    public class Aluno
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Sobrenome { get; set; } = string.Empty;
        public string DataNasc { get; set; } = string.Empty;
        public int ProfessorId { get; set; }
        public Professor? Professor { get; set; }
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
    }
}

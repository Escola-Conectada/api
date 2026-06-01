using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESCOLA_API.Models
{
    public class Professor
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
        public List<Aluno> Alunos { get; set; } = new();
    }
}

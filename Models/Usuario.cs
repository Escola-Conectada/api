using System.Collections.Generic;

namespace ESCOLA_API.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public DateOnly? DataNascimento { get; set; }
        public string? NomeMae { get; set; }
        public string? NomePai { get; set; }
        public string? Endereco { get; set; }
        public string Senha { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public int? IdUsuarioCriador { get; set; }
        public string? NomeUsuarioCriador { get; set; }
        public int IdPerfil { get; set; }
        public Perfil? Perfil { get; set; }
        public List<Aluno> Alunos { get; set; } = new();
        public List<Professor> Professores { get; set; } = new();
        public List<Diretoria> Diretorias { get; set; } = new();
        public List<Disciplina> DisciplinasMinistradas { get; set; } = new();
        public List<CadernetaDigital> CadernetasComoAluno { get; set; } = new();
        public List<UsuarioArquivo> Arquivos { get; set; } = new();
        public List<Holerite> Holerites { get; set; } = new();
        public List<Notificacao> Notificacoes { get; set; } = new();
    }
}

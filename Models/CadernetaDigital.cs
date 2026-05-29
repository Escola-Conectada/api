namespace ESCOLA_API.Models
{
    public class CadernetaDigital
    {
        public int IdCadernetaDigital { get; set; }
        public int IdAlunoUsuario { get; set; }
        public Usuario? AlunoUsuario { get; set; }
        public int? IdProfessorUsuario { get; set; }
        public Usuario? ProfessorUsuario { get; set; }
        public int? IdTipoEnsino { get; set; }
        public TipoEnsino? TipoEnsino { get; set; }
        public int? IdTurmaEnsino { get; set; }
        public TurmaEnsino? TurmaEnsino { get; set; }
        public int IdDisciplina { get; set; }
        public Disciplina? Disciplina { get; set; }
        public string Notas { get; set; } = string.Empty;
        public int Presencas { get; set; }
        public int Faltas { get; set; }
    }
}

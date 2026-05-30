namespace ESCOLA_API.Models
{
    public class AlunoTurmaEnsino
    {
        public int IdAlunoTurmaEnsino { get; set; }
        public int IdAlunoUsuario { get; set; }
        public Usuario? AlunoUsuario { get; set; }
        public int IdTurmaEnsino { get; set; }
        public TurmaEnsino? TurmaEnsino { get; set; }
        public int? IdUsuarioResponsavel { get; set; }
        public Usuario? UsuarioResponsavel { get; set; }
        public DateTime MatriculadoEmUtc { get; set; }
    }
}

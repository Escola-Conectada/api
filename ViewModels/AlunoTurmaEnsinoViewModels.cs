namespace ESCOLA_API.ViewModels
{
    public class AlunoTurmaEnsinoViewModel
    {
        public int IdAlunoTurmaEnsino { get; set; }
        public int IdAlunoUsuario { get; set; }
        public string NomeAluno { get; set; } = string.Empty;
        public string EmailAluno { get; set; } = string.Empty;
        public int IdTipoEnsino { get; set; }
        public string NomeTipoEnsino { get; set; } = string.Empty;
        public int IdTurmaEnsino { get; set; }
        public string NomeTurmaEnsino { get; set; } = string.Empty;
        public string CodigoTurma { get; set; } = string.Empty;
        public int? IdUsuarioResponsavel { get; set; }
        public string? NomeUsuarioResponsavel { get; set; }
        public DateTime MatriculadoEmUtc { get; set; }
    }

    public class AlunoTurmaEnsinoCreateUpdateViewModel
    {
        public int IdAlunoUsuario { get; set; }
        public int IdTurmaEnsino { get; set; }
    }
}

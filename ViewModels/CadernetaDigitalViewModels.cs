namespace ESCOLA_API.ViewModels
{
    public class DisciplinaViewModel
    {
        public int IdDisciplina { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int? IdProfessorUsuario { get; set; }
        public string NomeProfessor { get; set; } = string.Empty;
        public int? IdTipoEnsino { get; set; }
        public string? NomeTipoEnsino { get; set; }
        public int? IdTurmaEnsino { get; set; }
        public string? NomeTurmaEnsino { get; set; }
        public int? IdAreaConhecimento { get; set; }
        public string? NomeAreaConhecimento { get; set; }
        public string? Observacao { get; set; }
        public bool OfertaObrigatoria { get; set; } = true;
        public bool MatriculaFacultativa { get; set; }
        public int Ordem { get; set; }
    }

    public class DisciplinaCreateUpdateViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public int? IdTurmaEnsino { get; set; }
        public int? IdAreaConhecimento { get; set; }
        public string? Observacao { get; set; }
        public bool OfertaObrigatoria { get; set; } = true;
        public bool MatriculaFacultativa { get; set; }
    }

    public class TipoEnsinoCurricularViewModel
    {
        public int IdTipoEnsino { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public TurmaEnsinoCurricularViewModel[] Turmas { get; set; } = [];
    }

    public class TurmaEnsinoCurricularViewModel
    {
        public int IdTurmaEnsino { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public AreaConhecimentoCurricularViewModel[] AreasConhecimento { get; set; } = [];
    }

    public class AreaConhecimentoCurricularViewModel
    {
        public int IdAreaConhecimento { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public DisciplinaCurricularViewModel[] Disciplinas { get; set; } = [];
    }

    public class DisciplinaCurricularViewModel
    {
        public int IdDisciplina { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Observacao { get; set; }
        public bool OfertaObrigatoria { get; set; }
        public bool MatriculaFacultativa { get; set; }
        public int Ordem { get; set; }
    }

    public class CadernetaDigitalViewModel
    {
        public int IdCadernetaDigital { get; set; }
        public int IdAlunoUsuario { get; set; }
        public string NomeAluno { get; set; } = string.Empty;
        public string EmailAluno { get; set; } = string.Empty;
        public int IdDisciplina { get; set; }
        public string NomeDisciplina { get; set; } = string.Empty;
        public int? IdProfessorUsuario { get; set; }
        public string NomeProfessor { get; set; } = string.Empty;
        public int? IdTipoEnsino { get; set; }
        public string? NomeTipoEnsino { get; set; }
        public int? IdTurmaEnsino { get; set; }
        public string? NomeTurmaEnsino { get; set; }
        public int? IdAreaConhecimento { get; set; }
        public string? NomeAreaConhecimento { get; set; }
        public decimal[] Notas { get; set; } = [];
        public decimal MediaAritmetica { get; set; }
        public string Situacao { get; set; } = string.Empty;
        public string CorSituacao { get; set; } = string.Empty;
        public int Presencas { get; set; }
        public int Faltas { get; set; }
    }

    public class CadernetaDigitalCreateUpdateViewModel
    {
        public int IdAlunoUsuario { get; set; }
        public int IdTipoEnsino { get; set; }
        public int IdTurmaEnsino { get; set; }
        public int IdDisciplina { get; set; }
        public decimal[] Notas { get; set; } = [];
        public int Presencas { get; set; }
        public int Faltas { get; set; }
    }
}

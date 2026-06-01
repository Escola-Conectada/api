namespace ESCOLA_API.ViewModels
{
    public class BoletimDigitalViewModel
    {
        public int? IdBoletimDigital { get; set; }
        public int IdAlunoUsuario { get; set; }
        public string NomeAluno { get; set; } = string.Empty;
        public string EmailAluno { get; set; } = string.Empty;
        public int IdTipoEnsino { get; set; }
        public string NomeTipoEnsino { get; set; } = string.Empty;
        public int IdTurmaEnsino { get; set; }
        public string NomeTurmaEnsino { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool Completo { get; set; }
        public bool Liberado { get; set; }
        public bool PendenteDiretoria { get; set; }
        public bool PodeSolicitarLiberacao { get; set; }
        public bool PodeLiberar { get; set; }
        public bool PodeCompartilhar { get; set; }
        public int TotalDisciplinas { get; set; }
        public int DisciplinasLancadas { get; set; }
        public int DisciplinasPendentes { get; set; }
        public decimal? MediaGeral { get; set; }
        public string SituacaoGeral { get; set; } = string.Empty;
        public int TotalPresencas { get; set; }
        public int TotalFaltas { get; set; }
        public int? IdProfessorSolicitanteUsuario { get; set; }
        public string NomeProfessorSolicitante { get; set; } = string.Empty;
        public DateTime? SolicitadoEmUtc { get; set; }
        public int? IdAdministradorLiberacaoUsuario { get; set; }
        public string NomeAdministradorLiberacao { get; set; } = string.Empty;
        public DateTime? LiberadoEmUtc { get; set; }
        public BoletimDigitalDisciplinaViewModel[] Disciplinas { get; set; } = [];
    }

    public class BoletimDigitalDisciplinaViewModel
    {
        public int IdDisciplina { get; set; }
        public string NomeDisciplina { get; set; } = string.Empty;
        public int? IdProfessorUsuario { get; set; }
        public string NomeProfessor { get; set; } = string.Empty;
        public int? IdAreaConhecimento { get; set; }
        public string? NomeAreaConhecimento { get; set; }
        public bool OfertaObrigatoria { get; set; }
        public bool MatriculaFacultativa { get; set; }
        public int Ordem { get; set; }
        public bool Lancado { get; set; }
        public int? IdCadernetaDigital { get; set; }
        public decimal[] Notas { get; set; } = [];
        public decimal? MediaAritmetica { get; set; }
        public string Situacao { get; set; } = string.Empty;
        public string CorSituacao { get; set; } = string.Empty;
        public int? Presencas { get; set; }
        public int? Faltas { get; set; }
    }

    public class BoletimDigitalResumoAlunoViewModel
    {
        public int? IdBoletimDigital { get; set; }
        public int IdAlunoUsuario { get; set; }
        public string NomeAluno { get; set; } = string.Empty;
        public string EmailAluno { get; set; } = string.Empty;
        public int IdTurmaEnsino { get; set; }
        public string NomeTurmaEnsino { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool Completo { get; set; }
        public bool Liberado { get; set; }
        public bool PendenteLiberacao { get; set; }
        public int TotalDisciplinas { get; set; }
        public int DisciplinasLancadas { get; set; }
        public int DisciplinasPendentes { get; set; }
        public DateTime? SolicitadoEmUtc { get; set; }
        public string NomeProfessorSolicitante { get; set; } = string.Empty;
    }

    public class BoletimDigitalCompartilhamentoViewModel
    {
        public string Token { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public string EmailCompartilhamentoUrl { get; set; } = string.Empty;
        public string WhatsAppCompartilhamentoUrl { get; set; } = string.Empty;
        public DateTime ExpiraEmUtc { get; set; }
    }
}

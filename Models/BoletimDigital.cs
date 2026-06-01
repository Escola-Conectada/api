namespace ESCOLA_API.Models
{
    public class BoletimDigital
    {
        public int IdBoletimDigital { get; set; }
        public int IdAlunoUsuario { get; set; }
        public Usuario? AlunoUsuario { get; set; }
        public int IdTurmaEnsino { get; set; }
        public TurmaEnsino? TurmaEnsino { get; set; }
        public string Status { get; set; } = BoletimDigitalStatus.EmAberto;
        public int? IdProfessorSolicitanteUsuario { get; set; }
        public Usuario? ProfessorSolicitanteUsuario { get; set; }
        public DateTime? SolicitadoEmUtc { get; set; }
        public int? IdAdministradorLiberacaoUsuario { get; set; }
        public Usuario? AdministradorLiberacaoUsuario { get; set; }
        public DateTime? LiberadoEmUtc { get; set; }
        public DateTime AtualizadoEmUtc { get; set; } = DateTime.UtcNow;
    }

    public static class BoletimDigitalStatus
    {
        public const string EmAberto = "EmAberto";
        public const string PendenteDiretoria = "PendenteDiretoria";
        public const string Liberado = "Liberado";
    }
}

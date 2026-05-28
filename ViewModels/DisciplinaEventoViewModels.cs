namespace ESCOLA_API.ViewModels
{
    public class DisciplinaEventoViewModel
    {
        public int IdEventoDisciplina { get; set; }
        public int IdDisciplina { get; set; }
        public string NomeDisciplina { get; set; } = string.Empty;
        public int IdProfessorUsuario { get; set; }
        public string NomeProfessor { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateOnly Data { get; set; }
        public DateTime CriadoEmUtc { get; set; }
        public DateTime? AtualizadoEmUtc { get; set; }
    }

    public class DisciplinaEventoCreateUpdateViewModel
    {
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateOnly Data { get; set; }
    }
}

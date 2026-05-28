namespace ESCOLA_API.Models
{
    public class DisciplinaEvento
    {
        public int IdEventoDisciplina { get; set; }
        public int IdDisciplina { get; set; }
        public Disciplina? Disciplina { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateOnly Data { get; set; }
        public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEmUtc { get; set; }
    }
}

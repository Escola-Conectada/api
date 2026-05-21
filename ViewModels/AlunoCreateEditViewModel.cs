namespace form_API.ViewModels
{
    /// <summary>
    /// Dados para criar ou atualizar um aluno.
    /// </summary>
    public class AlunoCreateEditViewModel
    {
        /// <summary>
        /// Nome do aluno.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Sobrenome do aluno.
        /// </summary>
        public string Sobrenome { get; set; } = string.Empty;

        /// <summary>
        /// Data de nascimento no formato dd/MM/yyyy.
        /// </summary>
        public string DataNasc { get; set; } = string.Empty;

        /// <summary>
        /// Professor responsavel pelo aluno.
        /// </summary>
        public int ProfessorId { get; set; }

        /// <summary>
        /// Usuario vinculado ao aluno para login, quando existir.
        /// </summary>
        public int? IdUsuario { get; set; }
    }
}

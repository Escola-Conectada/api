namespace ESCOLA_API.ViewModels
{
    /// <summary>
    /// Dados para criar ou atualizar um professor.
    /// </summary>
    public class ProfessorCreateEditViewModel
    {
        /// <summary>
        /// Nome do professor.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Usuario vinculado ao professor para login.
        /// </summary>
        public int IdUsuario { get; set; }
    }
}

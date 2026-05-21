namespace form_API.ViewModels
{
    /// <summary>
    /// Dados para criar ou atualizar um integrante da diretoria.
    /// </summary>
    public class DiretoriaCreateEditViewModel
    {
        /// <summary>
        /// Nome do integrante da diretoria.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Usuario vinculado ao integrante para login, quando existir.
        /// </summary>
        public int? IdUsuario { get; set; }
    }

    /// <summary>
    /// Dados retornados para um integrante da diretoria.
    /// </summary>
    public class DiretoriaViewModel
    {
        /// <summary>
        /// Identificador da diretoria.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do integrante da diretoria.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do usuario vinculado.
        /// </summary>
        public int? IdUsuario { get; set; }

        /// <summary>
        /// Dados resumidos do usuario vinculado.
        /// </summary>
        public UsuarioSummaryViewModel? Usuario { get; set; }
    }
}

namespace form_API.ViewModels
{
    /// <summary>
    /// Dados para criacao de usuario.
    /// </summary>
    public class UsuarioCreateViewModel
    {
        /// <summary>
        /// Nome do usuario.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email usado para login.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefone de contato.
        /// </summary>
        public string Telefone { get; set; } = string.Empty;

        /// <summary>
        /// Senha em texto puro, armazenada como hash pela API.
        /// </summary>
        public string Senha { get; set; } = string.Empty;

        /// <summary>
        /// Perfil de autorizacao vinculado ao usuario.
        /// </summary>
        public int IdPerfil { get; set; }
    }

    /// <summary>
    /// Dados publicos de um usuario, sem expor a senha.
    /// </summary>
    public class UsuarioSummaryViewModel
    {
        /// <summary>
        /// Identificador do usuario.
        /// </summary>
        public int IdUsuario { get; set; }

        /// <summary>
        /// Nome do usuario.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email usado para login.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefone de contato.
        /// </summary>
        public string Telefone { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do perfil de autorizacao.
        /// </summary>
        public int IdPerfil { get; set; }

        /// <summary>
        /// Descricao do perfil de autorizacao.
        /// </summary>
        public string DescricaoPerfil { get; set; } = string.Empty;
    }

    /// <summary>
    /// Perfil de autorizacao disponivel para usuarios.
    /// </summary>
    public class PerfilViewModel
    {
        /// <summary>
        /// Identificador do perfil.
        /// </summary>
        public int IdPerfil { get; set; }

        /// <summary>
        /// Descricao do perfil.
        /// </summary>
        public string DescricaoPerfil { get; set; } = string.Empty;
    }
}

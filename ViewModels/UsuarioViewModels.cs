namespace ESCOLA_API.ViewModels
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
        /// Tipo do usuario: Aluno, Professor ou Administrador.
        /// </summary>
        public string TipoUsuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados para atualizacao de usuario.
    /// </summary>
    public class UsuarioUpdateViewModel
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
        /// Tipo do usuario: Aluno, Professor ou Administrador. Apenas administradores podem alterar.
        /// </summary>
        public string? TipoUsuario { get; set; }
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

        /// <summary>
        /// Tipo do usuario.
        /// </summary>
        public string TipoUsuario { get; set; } = string.Empty;
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

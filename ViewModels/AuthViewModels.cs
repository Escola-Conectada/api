namespace form_API.ViewModels
{
    /// <summary>
    /// Dados enviados para autenticar um usuario.
    /// </summary>
    public class LoginRequestViewModel
    {
        /// <summary>
        /// Email cadastrado do usuario.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha em texto puro enviada somente no login.
        /// </summary>
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta de autenticacao com token JWT e dados resumidos do usuario.
    /// </summary>
    public class AuthResponseViewModel
    {
        /// <summary>
        /// Token JWT usado no cabecalho Authorization.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora UTC de expiracao do token.
        /// </summary>
        public DateTime ExpiraEm { get; set; }

        /// <summary>
        /// Dados do usuario autenticado.
        /// </summary>
        public UsuarioSummaryViewModel Usuario { get; set; } = new();

        /// <summary>
        /// Indica se o usuario ainda usa a senha padrao e deve troca-la.
        /// </summary>
        public bool DeveAlterarSenhaPadrao { get; set; }
    }

    /// <summary>
    /// Dados para alteracao de senha do usuario autenticado.
    /// </summary>
    public class AlterarSenhaViewModel
    {
        /// <summary>
        /// Senha atual do usuario.
        /// </summary>
        public string SenhaAtual { get; set; } = string.Empty;

        /// <summary>
        /// Nova senha escolhida pelo usuario.
        /// </summary>
        public string NovaSenha { get; set; } = string.Empty;

        /// <summary>
        /// Confirmacao da nova senha.
        /// </summary>
        public string ConfirmacaoSenha { get; set; } = string.Empty;
    }
}

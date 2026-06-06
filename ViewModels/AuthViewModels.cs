namespace ESCOLA_API.ViewModels
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
    /// Dados enviados para autenticar um usuario com Google Identity Services.
    /// </summary>
    public class GoogleLoginRequestViewModel
    {
        /// <summary>
        /// ID token JWT retornado pelo login do Google.
        /// </summary>
        public string IdToken { get; set; } = string.Empty;
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

    /// <summary>
    /// Dados enviados para solicitar redefinicao de senha.
    /// </summary>
    public class EsqueciSenhaViewModel
    {
        /// <summary>
        /// Email cadastrado do usuario que solicitou a redefinicao da senha.
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta publica da solicitacao de redefinicao de senha.
    /// </summary>
    public class EsqueciSenhaResponseViewModel
    {
        /// <summary>
        /// Mensagem generica para evitar enumeracao de contas.
        /// </summary>
        public string Mensagem { get; set; } = string.Empty;

        /// <summary>
        /// Token retornado apenas em ambiente de desenvolvimento.
        /// </summary>
        public string? TokenRedefinicao { get; set; }

        /// <summary>
        /// Data e hora UTC de expiracao do token.
        /// </summary>
        public DateTime? ExpiraEmUtc { get; set; }
    }

    /// <summary>
    /// Resultado interno da solicitacao de redefinicao de senha.
    /// </summary>
    public class RedefinicaoSenhaSolicitadaViewModel
    {
        /// <summary>
        /// Indica se existe usuario para o email informado.
        /// </summary>
        public bool UsuarioEncontrado { get; set; }

        /// <summary>
        /// Token temporario gerado para redefinir a senha.
        /// </summary>
        public string? TokenRedefinicao { get; set; }

        /// <summary>
        /// Data e hora UTC de expiracao do token.
        /// </summary>
        public DateTime? ExpiraEmUtc { get; set; }
    }

    /// <summary>
    /// Dados para concluir a redefinicao de senha com token temporario.
    /// </summary>
    public class RedefinirSenhaViewModel
    {
        /// <summary>
        /// Email cadastrado do usuario.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Token temporario recebido no fluxo de redefinicao.
        /// </summary>
        public string Token { get; set; } = string.Empty;

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

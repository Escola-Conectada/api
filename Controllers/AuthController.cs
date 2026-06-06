using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESCOLA_API.Controllers
{
    /// <summary>
    /// Operacoes de autenticacao e autorizacao.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IHostEnvironment environment, ILogger<AuthController> logger)
        {
            _authService = authService;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Autentica um usuario e retorna um token JWT.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginRequestViewModel model)
        {
            var result = await _authService.LoginAsync(model);
            if (result == null)
            {
                _logger.LogWarning("Tentativa de login recusada para {Email}", model.Email);
                return Unauthorized("Email ou senha invalidos.");
            }

            _logger.LogInformation(
                "Login realizado para usuario {UsuarioId} com perfil {Perfil}",
                result.Usuario.IdUsuario,
                result.Usuario.DescricaoPerfil);
            return Ok(result);
        }

        /// <summary>
        /// Autentica um usuario com o ID token do Google e retorna um token JWT da aplicacao.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("google")]
        [ProducesResponseType(typeof(AuthResponseViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> LoginGoogle(GoogleLoginRequestViewModel model)
        {
            try
            {
                var result = await _authService.LoginGoogleAsync(model);
                if (result == null)
                {
                    _logger.LogWarning("Tentativa de login com Google recusada.");
                    return Unauthorized("Conta Google nao autorizada.");
                }

                _logger.LogInformation(
                    "Login com Google realizado para usuario {UsuarioId} com perfil {Perfil}",
                    result.Usuario.IdUsuario,
                    result.Usuario.DescricaoPerfil);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Login com Google indisponivel por configuracao ausente.");
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    "Login com Google nao configurado.");
            }
        }

        /// <summary>
        /// Retorna os dados do usuario autenticado.
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Me()
        {
            var usuario = await _authService.GetUsuarioAtualAsync(User);
            if (usuario == null)
            {
                _logger.LogWarning("Requisicao /me sem usuario autenticado valido.");
                return Unauthorized();
            }

            return Ok(usuario);
        }

        /// <summary>
        /// Verifica se o token JWT e valido.
        /// </summary>
        [Authorize]
        [HttpGet("autorizar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Autorizar()
        {
            return Ok(new { autorizado = true });
        }

        /// <summary>
        /// Verifica se o usuario autenticado possui perfil de administrador.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpGet("autorizar/admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult AutorizarAdministrador()
        {
            return Ok(new { autorizado = true, perfil = "Administrador" });
        }

        /// <summary>
        /// Altera a senha do usuario autenticado.
        /// </summary>
        [Authorize]
        [HttpPost("alterar-senha")]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel model)
        {
            try
            {
                var usuario = await _authService.AlterarSenhaAsync(User, model);
                if (usuario == null)
                {
                    _logger.LogWarning("Tentativa de alteracao de senha sem usuario autenticado valido.");
                    return Unauthorized();
                }

                _logger.LogInformation("Senha alterada para usuario {UsuarioId}", usuario.IdUsuario);
                return Ok(usuario);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Alteracao de senha recusada por regra de negocio.");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Solicita um token temporario para redefinicao de senha.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("esqueci-senha")]
        [ProducesResponseType(typeof(EsqueciSenhaResponseViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EsqueciSenha(EsqueciSenhaViewModel model)
        {
            var result = await _authService.SolicitarRedefinicaoSenhaAsync(model);

            if (result.UsuarioEncontrado)
            {
                _logger.LogInformation("Token de redefinicao de senha gerado no fluxo de esqueci senha.");
            }
            else
            {
                _logger.LogWarning("Solicitacao de esqueci senha recebida para email nao cadastrado.");
            }

            return Ok(new EsqueciSenhaResponseViewModel
            {
                Mensagem = "Se o email informado estiver cadastrado, enviaremos as instrucoes de redefinicao de senha.",
                TokenRedefinicao = _environment.IsDevelopment() ? result.TokenRedefinicao : null,
                ExpiraEmUtc = _environment.IsDevelopment() ? result.ExpiraEmUtc : null
            });
        }

        /// <summary>
        /// Redefine a senha usando o token temporario recebido.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("redefinir-senha")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RedefinirSenha(RedefinirSenhaViewModel model)
        {
            try
            {
                var redefinida = await _authService.RedefinirSenhaAsync(model);
                if (!redefinida)
                {
                    return BadRequest("Token invalido ou expirado.");
                }

                _logger.LogInformation("Senha redefinida com token temporario.");
                return Ok(new { mensagem = "Senha redefinida com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Redefinicao de senha recusada por regra de negocio.");
                return BadRequest(ex.Message);
            }
        }
    }
}

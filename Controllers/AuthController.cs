using form_API.Services;
using form_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace form_API.Controllers
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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
                return Unauthorized("Email ou senha invalidos.");
            }

            return Ok(result);
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
                    return Unauthorized();
                }

                return Ok(usuario);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

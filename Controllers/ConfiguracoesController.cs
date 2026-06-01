using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESCOLA_API.Controllers
{
    [Route("api/configuracoes")]
    [ApiController]
    [Produces("application/json")]
    public class ConfiguracoesController : ControllerBase
    {
        private readonly IConfiguracaoAplicacaoService _service;
        private readonly ILogger<ConfiguracoesController> _logger;

        public ConfiguracoesController(
            IConfiguracaoAplicacaoService service,
            ILogger<ConfiguracoesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("aplicacao")]
        [ProducesResponseType(typeof(ConfiguracaoAplicacaoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAplicacao(CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await _service.GetAsync(cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configuracoes da aplicacao");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("aplicacao")]
        [ProducesResponseType(typeof(ConfiguracaoAplicacaoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutAplicacao(
            ConfiguracaoAplicacaoUpdateViewModel model,
            CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await _service.UpdateAsync(model, User, cancellationToken));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar configuracoes da aplicacao");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }
    }
}

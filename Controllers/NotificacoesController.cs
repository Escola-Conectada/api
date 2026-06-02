using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESCOLA_API.Controllers
{
    [Authorize]
    [Route("api/notificacoes")]
    [ApiController]
    [Produces("application/json")]
    public class NotificacoesController : ControllerBase
    {
        private readonly INotificacaoService _notificacaoService;
        private readonly ILogger<NotificacoesController> _logger;

        public NotificacoesController(
            INotificacaoService notificacaoService,
            ILogger<NotificacoesController> logger)
        {
            _notificacaoService = notificacaoService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(NotificacaoViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMinhas(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20)
        {
            try
            {
                return Ok(await _notificacaoService.GetMinhasAsync(User, pagina, tamanhoPagina));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter notificacoes do usuario logado");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpGet("nao-lidas/contador")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CountNaoLidas()
        {
            try
            {
                return Ok(new { total = await _notificacaoService.CountNaoLidasAsync(User) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar notificacoes nao lidas do usuario logado");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ProducesResponseType(typeof(NotificacaoViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(NotificacaoCreateViewModel model)
        {
            try
            {
                var created = await _notificacaoService.AddAsync(model, User);
                return CreatedAtAction(nameof(GetMinhas), null, created);
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
                _logger.LogError(ex, "Erro ao criar notificacao manual");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Envia uma notificacao manual para todos os usuarios dos perfis informados.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpPost("perfis")]
        [ProducesResponseType(typeof(NotificacaoEnvioViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostPerfis(NotificacaoPerfisCreateViewModel model)
        {
            try
            {
                var created = await _notificacaoService.AddParaPerfisAsync(model, User);
                return CreatedAtAction(nameof(GetMinhas), null, created);
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
                _logger.LogError(ex, "Erro ao criar notificacoes manuais por perfil");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Envia uma notificacao manual do administrador para todos os perfis cadastrados.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpPost("todos")]
        [ProducesResponseType(typeof(NotificacaoEnvioViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostTodos(NotificacaoPerfisCreateViewModel model)
        {
            try
            {
                model.TodosOsPerfis = true;
                var created = await _notificacaoService.AddParaPerfisAsync(model, User);
                return CreatedAtAction(nameof(GetMinhas), null, created);
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
                _logger.LogError(ex, "Erro ao criar notificacoes manuais para todos os perfis");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpPatch("{notificacaoId:int}/lida")]
        [ProducesResponseType(typeof(NotificacaoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarcarComoLida(int notificacaoId)
        {
            try
            {
                var updated = await _notificacaoService.MarcarComoLidaAsync(notificacaoId, User);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar notificacao {NotificacaoId} como lida", notificacaoId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpPatch("lidas")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarcarTodasComoLidas()
        {
            try
            {
                return Ok(new { total = await _notificacaoService.MarcarTodasComoLidasAsync(User) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar todas as notificacoes como lidas");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }
    }
}

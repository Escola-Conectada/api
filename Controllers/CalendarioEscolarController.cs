using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESCOLA_API.Controllers
{
    /// <summary>
    /// Painel de calendario escolar anual com feriados nacionais brasileiros.
    /// </summary>
    [Authorize]
    [Route("api/calendario-escolar")]
    [ApiController]
    [Produces("application/json")]
    public class CalendarioEscolarController : ControllerBase
    {
        private readonly ICalendarioEscolarService _service;
        private readonly ILogger<CalendarioEscolarController> _logger;

        public CalendarioEscolarController(
            ICalendarioEscolarService service,
            ILogger<CalendarioEscolarController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CalendarioEscolarAnoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int? ano, [FromQuery] int? mesSelecionado)
        {
            try
            {
                return Ok(_service.GetCalendarioAnual(ano, mesSelecionado));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar calendario escolar");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }
    }
}

using ESCOLA_API.Services;
using ESCOLA_API.Security;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESCOLA_API.Controllers
{
    /// <summary>
    /// Operacoes da caderneta digital.
    /// </summary>
    [Authorize]
    [Route("api/caderneta-digital")]
    [ApiController]
    [Produces("application/json")]
    public class CadernetaDigitalController : ControllerBase
    {
        private readonly ICadernetaDigitalService _service;
        private readonly IDisciplinaEventoService _eventoService;
        private readonly ILogger<CadernetaDigitalController> _logger;

        public CadernetaDigitalController(
            ICadernetaDigitalService service,
            IDisciplinaEventoService eventoService,
            ILogger<CadernetaDigitalController> logger)
        {
            _service = service;
            _eventoService = eventoService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CadernetaDigitalViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await _service.GetAllAsync(User));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter caderneta digital");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpGet("{cadernetaId:int}")]
        [ProducesResponseType(typeof(CadernetaDigitalViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int cadernetaId)
        {
            try
            {
                var caderneta = await _service.GetByIdAsync(cadernetaId, User);
                return caderneta == null ? NotFound() : Ok(caderneta);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter lancamento da caderneta {CadernetaId}", cadernetaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(CadernetaDigitalViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(CadernetaDigitalCreateUpdateViewModel model)
        {
            try
            {
                var created = await _service.AddAsync(model, User);
                return CreatedAtAction(nameof(GetById), new { cadernetaId = created.IdCadernetaDigital }, created);
            }
            catch (InvalidSessionException ex)
            {
                return Unauthorized(ex.Message);
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
                _logger.LogError(ex, "Erro ao criar lancamento da caderneta");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpPut("{cadernetaId:int}")]
        [ProducesResponseType(typeof(CadernetaDigitalViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int cadernetaId, CadernetaDigitalCreateUpdateViewModel model)
        {
            try
            {
                var updated = await _service.UpdateAsync(cadernetaId, model, User);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (InvalidSessionException ex)
            {
                return Unauthorized(ex.Message);
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
                _logger.LogError(ex, "Erro ao atualizar lancamento da caderneta {CadernetaId}", cadernetaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpDelete("{cadernetaId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int cadernetaId)
        {
            try
            {
                var deleted = await _service.DeleteAsync(cadernetaId, User);
                return deleted ? Ok() : NotFound();
            }
            catch (InvalidSessionException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir lancamento da caderneta {CadernetaId}", cadernetaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpGet("disciplinas")]
        [ProducesResponseType(typeof(DisciplinaViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDisciplinas()
        {
            try
            {
                return Ok(await _service.GetDisciplinasAsync(User));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter disciplinas da caderneta");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpPost("disciplinas")]
        [ProducesResponseType(typeof(DisciplinaViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostDisciplina(DisciplinaCreateUpdateViewModel model)
        {
            try
            {
                var created = await _service.AddDisciplinaAsync(model, User);
                return CreatedAtAction(nameof(GetDisciplinas), null, created);
            }
            catch (InvalidSessionException ex)
            {
                return Unauthorized(ex.Message);
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
                _logger.LogError(ex, "Erro ao criar disciplina da caderneta");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpPut("disciplinas/{disciplinaId:int}")]
        [ProducesResponseType(typeof(DisciplinaViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutDisciplina(int disciplinaId, DisciplinaCreateUpdateViewModel model)
        {
            try
            {
                var updated = await _service.UpdateDisciplinaAsync(disciplinaId, model, User);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (InvalidSessionException ex)
            {
                return Unauthorized(ex.Message);
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
                _logger.LogError(ex, "Erro ao atualizar disciplina {DisciplinaId}", disciplinaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpDelete("disciplinas/{disciplinaId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDisciplina(int disciplinaId)
        {
            try
            {
                var deleted = await _service.DeleteDisciplinaAsync(disciplinaId, User);
                return deleted ? Ok() : NotFound();
            }
            catch (InvalidSessionException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir disciplina {DisciplinaId}", disciplinaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Lista eventos de disciplinas, como avaliacoes e entregas de trabalhos, filtrando por disciplina, ano e mes.
        /// </summary>
        /// <param name="idDisciplina">Identificador da disciplina para filtrar. Quando omitido, retorna as disciplinas permitidas ao usuario.</param>
        /// <param name="ano">Ano dos eventos desejados.</param>
        /// <param name="mes">Mes dos eventos desejados.</param>
        [HttpGet("disciplinas/eventos")]
        [ProducesResponseType(typeof(DisciplinaEventoViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEventosDisciplina(
            [FromQuery] int? idDisciplina,
            [FromQuery] int? ano,
            [FromQuery] int? mes)
        {
            try
            {
                return Ok(await _eventoService.GetEventosAsync(User, idDisciplina, ano, mes));
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
                _logger.LogError(ex, "Erro ao obter eventos das disciplinas");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Cria uma data de avaliacao ou entrega de trabalho para uma disciplina do professor logado e notifica os alunos matriculados.
        /// </summary>
        /// <param name="disciplinaId">Identificador da disciplina do professor.</param>
        /// <param name="model">Dados do evento escolar.</param>
        [HttpPost("disciplinas/{disciplinaId:int}/eventos")]
        [ProducesResponseType(typeof(DisciplinaEventoViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostEventoDisciplina(int disciplinaId, DisciplinaEventoCreateUpdateViewModel model)
        {
            try
            {
                var created = await _eventoService.AddAsync(disciplinaId, model, User);
                return CreatedAtAction(nameof(GetEventosDisciplina), new { idDisciplina = disciplinaId }, created);
            }
            catch (InvalidSessionException ex)
            {
                return Unauthorized(ex.Message);
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
                _logger.LogError(ex, "Erro ao criar evento da disciplina {DisciplinaId}", disciplinaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Atualiza uma data de avaliacao ou entrega de trabalho de uma disciplina do professor logado e notifica os alunos matriculados.
        /// </summary>
        /// <param name="disciplinaId">Identificador da disciplina do professor.</param>
        /// <param name="eventoId">Identificador do evento da disciplina.</param>
        /// <param name="model">Novos dados do evento escolar.</param>
        [HttpPut("disciplinas/{disciplinaId:int}/eventos/{eventoId:int}")]
        [ProducesResponseType(typeof(DisciplinaEventoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutEventoDisciplina(
            int disciplinaId,
            int eventoId,
            DisciplinaEventoCreateUpdateViewModel model)
        {
            try
            {
                var updated = await _eventoService.UpdateAsync(disciplinaId, eventoId, model, User);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (InvalidSessionException ex)
            {
                return Unauthorized(ex.Message);
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
                _logger.LogError(ex, "Erro ao atualizar evento {EventoId} da disciplina {DisciplinaId}", eventoId, disciplinaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Remove uma data de avaliacao ou entrega de trabalho de uma disciplina do professor logado.
        /// </summary>
        /// <param name="disciplinaId">Identificador da disciplina do professor.</param>
        /// <param name="eventoId">Identificador do evento da disciplina.</param>
        [HttpDelete("disciplinas/{disciplinaId:int}/eventos/{eventoId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEventoDisciplina(int disciplinaId, int eventoId)
        {
            try
            {
                var deleted = await _eventoService.DeleteAsync(disciplinaId, eventoId, User);
                return deleted ? Ok() : NotFound();
            }
            catch (InvalidSessionException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir evento {EventoId} da disciplina {DisciplinaId}", eventoId, disciplinaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }
    }
}

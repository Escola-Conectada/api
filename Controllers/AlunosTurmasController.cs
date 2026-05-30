using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESCOLA_API.Controllers
{
    /// <summary>
    /// Operacoes para matricular alunos em turmas.
    /// </summary>
    [Authorize]
    [Route("api/alunos-turmas")]
    [ApiController]
    [Produces("application/json")]
    public class AlunosTurmasController : ControllerBase
    {
        private readonly IAlunoTurmaEnsinoService _service;
        private readonly ILogger<AlunosTurmasController> _logger;

        public AlunosTurmasController(
            IAlunoTurmaEnsinoService service,
            ILogger<AlunosTurmasController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(AlunoTurmaEnsinoViewModel[]), StatusCodes.Status200OK)]
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
                _logger.LogError(ex, "Erro ao obter matriculas de alunos em turmas");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpGet("{matriculaId:int}")]
        [ProducesResponseType(typeof(AlunoTurmaEnsinoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int matriculaId)
        {
            try
            {
                var matricula = await _service.GetByIdAsync(matriculaId, User);
                return matricula == null ? NotFound() : Ok(matricula);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter matricula {MatriculaId}", matriculaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(AlunoTurmaEnsinoViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(AlunoTurmaEnsinoCreateUpdateViewModel model)
        {
            try
            {
                var created = await _service.AddAsync(model, User);
                return CreatedAtAction(nameof(GetById), new { matriculaId = created.IdAlunoTurmaEnsino }, created);
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
                _logger.LogError(ex, "Erro ao criar matricula de aluno em turma");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpPut("{matriculaId:int}")]
        [ProducesResponseType(typeof(AlunoTurmaEnsinoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int matriculaId, AlunoTurmaEnsinoCreateUpdateViewModel model)
        {
            try
            {
                var updated = await _service.UpdateAsync(matriculaId, model, User);
                return updated == null ? NotFound() : Ok(updated);
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
                _logger.LogError(ex, "Erro ao atualizar matricula {MatriculaId}", matriculaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [HttpDelete("{matriculaId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int matriculaId)
        {
            try
            {
                var deleted = await _service.DeleteAsync(matriculaId, User);
                return deleted ? Ok() : NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir matricula {MatriculaId}", matriculaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }
    }
}

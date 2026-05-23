using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESCOLA_API.Controllers
{
    /// <summary>
    /// Operacoes para consulta e cadastro de usuarios.
    /// </summary>
    [Authorize]
    [Route("api/usuarios")]
    [ApiController]
    [Produces("application/json")]
    public class UsuariosController : ControllerBase
    {
        private readonly ILogger<UsuariosController> _logger;
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        /// <summary>
        /// Lista usuarios cadastrados para vinculos com outras tabelas.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var usuarios = await _usuarioService.GetAllAsync(User);
                return Ok(usuarios);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Busca um usuario pelo identificador.
        /// </summary>
        [HttpGet("{usuarioId:int}")]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            try
            {
                var usuario = await _usuarioService.GetByIdAsync(usuarioId, User);
                if (usuario == null) return NotFound();
                return Ok(usuario);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuario por id {UsuarioId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Lista perfis disponiveis para cadastro de usuario.
        /// </summary>
        [HttpGet("perfis")]
        [ProducesResponseType(typeof(PerfilViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPerfis()
        {
            try
            {
                var perfis = await _usuarioService.GetPerfisAsync(User);
                return Ok(perfis);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter perfis");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Cadastra um novo usuario.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(UsuarioCreateViewModel model)
        {
            try
            {
                var created = await _usuarioService.AddAsync(model, User);
                return CreatedAtAction(nameof(GetByUsuarioId), new { usuarioId = created.IdUsuario }, created);
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
                _logger.LogError(ex, "Erro ao criar usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Atualiza um usuario existente.
        /// </summary>
        [HttpPut("{usuarioId:int}")]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int usuarioId, UsuarioUpdateViewModel model)
        {
            try
            {
                var updated = await _usuarioService.UpdateAsync(usuarioId, model, User);
                if (updated == null) return NotFound();
                return Ok(updated);
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
                _logger.LogError(ex, "Erro ao atualizar usuario {UsuarioId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Exclui um usuario.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{usuarioId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int usuarioId)
        {
            try
            {
                var deleted = await _usuarioService.DeleteAsync(usuarioId);
                if (!deleted) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuario {UsuarioId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }
    }
}

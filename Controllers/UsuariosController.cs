using form_API.Services;
using form_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace form_API.Controllers
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
        [Authorize(Roles = "Administrador,Contribuinte")]
        [HttpGet]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var usuarios = await _usuarioService.GetAllAsync();
                return Ok(usuarios);
            }
            catch
            {
                _logger.LogError("Erro ao obter usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Busca um usuario pelo identificador.
        /// </summary>
        [Authorize(Roles = "Administrador,Contribuinte")]
        [HttpGet("{usuarioId}")]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            try
            {
                var usuario = await _usuarioService.GetByIdAsync(usuarioId);
                if (usuario == null) return NotFound();
                return Ok(usuario);
            }
            catch
            {
                _logger.LogError("Erro ao obter usuario por id");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Lista perfis disponiveis para cadastro de usuario.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpGet("perfis")]
        [ProducesResponseType(typeof(PerfilViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPerfis()
        {
            try
            {
                var perfis = await _usuarioService.GetPerfisAsync();
                return Ok(perfis);
            }
            catch
            {
                _logger.LogError("Erro ao obter perfis");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Cadastra um novo usuario.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(UsuarioCreateViewModel model)
        {
            try
            {
                var created = await _usuarioService.AddAsync(model);
                return CreatedAtAction(nameof(GetByUsuarioId), new { usuarioId = created.IdUsuario }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                _logger.LogError("Erro ao criar usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Atualiza um usuario existente.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpPut("{usuarioId}")]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int usuarioId, UsuarioCreateViewModel model)
        {
            try
            {
                var updated = await _usuarioService.UpdateAsync(usuarioId, model);
                if (updated == null) return NotFound();
                return CreatedAtAction(nameof(GetByUsuarioId), new { usuarioId = updated.IdUsuario }, updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                _logger.LogError("Erro ao atualizar usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Exclui um usuario.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{usuarioId}")]
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
            catch
            {
                _logger.LogError("Erro ao excluir usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }
    }
}

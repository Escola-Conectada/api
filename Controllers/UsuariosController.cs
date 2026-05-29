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
        private readonly IUsuarioArquivoService _arquivoService;
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(
            IUsuarioService usuarioService,
            IUsuarioArquivoService arquivoService,
            ILogger<UsuariosController> logger)
        {
            _usuarioService = usuarioService;
            _arquivoService = arquivoService;
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

        /// <summary>
        /// Solicita a exclusao da conta do usuario autenticado.
        /// </summary>
        [HttpPost("me/exclusao-conta")]
        [ProducesResponseType(typeof(ExclusaoContaSolicitadaViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SolicitarExclusaoConta(SolicitarExclusaoContaViewModel model)
        {
            try
            {
                var solicitacao = await _usuarioService.SolicitarExclusaoContaAsync(User, model);
                if (solicitacao == null) return Unauthorized();
                return Ok(solicitacao);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao solicitar exclusao de conta");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Lista solicitacoes de exclusao de conta pendentes.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpGet("exclusoes-conta")]
        [ProducesResponseType(typeof(ExclusaoContaSolicitadaViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSolicitacoesExclusaoConta()
        {
            try
            {
                var solicitacoes = await _usuarioService.GetSolicitacoesExclusaoContaAsync(User);
                return Ok(solicitacoes);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar solicitacoes de exclusao de conta");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Obtem a foto de perfil do usuario.
        /// </summary>
        [HttpGet("{usuarioId:int}/foto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFoto(int usuarioId)
        {
            try
            {
                var arquivo = await _arquivoService.DownloadFotoAsync(usuarioId, User);
                if (arquivo == null) return NotFound();
                return File(arquivo.Stream, arquivo.ContentType, arquivo.NomeArquivo);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter foto do usuario {UsuarioId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Lista arquivos vinculados ao usuario.
        /// </summary>
        [HttpGet("{usuarioId:int}/arquivos")]
        [ProducesResponseType(typeof(UsuarioArquivoViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetArquivos(int usuarioId)
        {
            try
            {
                var arquivos = await _arquivoService.GetArquivosAsync(usuarioId, User);
                return Ok(arquivos);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter arquivos do usuario {UsuarioId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Baixa um arquivo vinculado ao usuario.
        /// </summary>
        [HttpGet("{usuarioId:int}/arquivos/{arquivoId:int}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadArquivo(int usuarioId, int arquivoId)
        {
            try
            {
                var arquivo = await _arquivoService.DownloadArquivoAsync(usuarioId, arquivoId, User);
                if (arquivo == null) return NotFound();
                return File(arquivo.Stream, arquivo.ContentType, arquivo.NomeArquivo);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao baixar arquivo {ArquivoId} do usuario {UsuarioId}", arquivoId, usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Atualiza a foto de perfil do usuario.
        /// </summary>
        [HttpPost("{usuarioId:int}/foto")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UsuarioSummaryViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFoto(int usuarioId, [FromForm] IFormFile? arquivo)
        {
            try
            {
                if (arquivo == null)
                {
                    return BadRequest("Informe o arquivo da foto.");
                }

                var updated = await _arquivoService.UploadFotoAsync(usuarioId, arquivo, User);
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
                _logger.LogError(ex, "Erro ao enviar foto do usuario {UsuarioId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Adiciona um certificado em PDF ao perfil do professor.
        /// </summary>
        [HttpPost("{usuarioId:int}/certificados")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UsuarioArquivoViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadCertificado(int usuarioId, [FromForm] IFormFile? arquivo)
        {
            try
            {
                if (arquivo == null)
                {
                    return BadRequest("Informe o arquivo do certificado.");
                }

                var created = await _arquivoService.UploadCertificadoAsync(usuarioId, arquivo, User);
                return CreatedAtAction(nameof(GetArquivos), new { usuarioId }, created);
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
                _logger.LogError(ex, "Erro ao enviar certificado do usuario {UsuarioId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Remove um arquivo vinculado ao usuario.
        /// </summary>
        [HttpDelete("{usuarioId:int}/arquivos/{arquivoId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteArquivo(int usuarioId, int arquivoId)
        {
            try
            {
                var deleted = await _arquivoService.DeleteArquivoAsync(usuarioId, arquivoId, User);
                if (!deleted) return NotFound();
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir arquivo {ArquivoId} do usuario {UsuarioId}", arquivoId, usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }
    }
}

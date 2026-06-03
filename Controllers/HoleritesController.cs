using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace ESCOLA_API.Controllers
{
    /// <summary>
    /// Operacoes para consulta e gerenciamento de holerites de funcionarios.
    /// Administradores enviam e gerenciam PDFs; professores e administradores consultam apenas os proprios holerites.
    /// </summary>
    [Authorize]
    [Route("api/holerites")]
    [ApiController]
    [Produces("application/json")]
    public class HoleritesController : ControllerBase
    {
        private readonly IHoleriteService _holeriteService;
        private readonly ILogger<HoleritesController> _logger;

        public HoleritesController(
            IHoleriteService holeriteService,
            ILogger<HoleritesController> logger)
        {
            _holeriteService = holeriteService;
            _logger = logger;
        }

        /// <summary>
        /// Lista os holerites do funcionario logado. Disponivel para professores e administradores.
        /// </summary>
        [Authorize(Roles = "Administrador,Professor")]
        [HttpGet("me")]
        [ProducesResponseType(typeof(HoleriteViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMeusHolerites()
        {
            try
            {
                return Ok(await _holeriteService.GetMeusHoleritesAsync(User));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar holerites do usuario logado");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Baixa um holerite do funcionario logado. Disponivel para professores e administradores.
        /// </summary>
        [Authorize(Roles = "Administrador,Professor")]
        [HttpGet("me/{holeriteId:int}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadMeuHolerite(int holeriteId)
        {
            try
            {
                var arquivo = await _holeriteService.DownloadMeuHoleriteAsync(holeriteId, User);
                if (arquivo == null) return NotFound();

                return File(arquivo.Stream, arquivo.ContentType, arquivo.NomeArquivo);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao baixar holerite {HoleriteId} do usuario logado", holeriteId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Gera um link temporario para compartilhar um holerite do funcionario logado.
        /// </summary>
        [Authorize(Roles = "Administrador,Professor")]
        [HttpPost("me/{holeriteId:int}/compartilhamento")]
        [ProducesResponseType(typeof(HoleriteCompartilhamentoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompartilharMeuHolerite(int holeriteId)
        {
            try
            {
                var compartilhamento = await _holeriteService.CriarCompartilhamentoMeuHoleriteAsync(holeriteId, User);
                if (compartilhamento == null) return NotFound();

                compartilhamento.Url = CriarUrlCompartilhamento(compartilhamento.Token);
                return Ok(compartilhamento);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao compartilhar holerite {HoleriteId} do usuario logado", holeriteId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Lista holerites de um funcionario. Disponivel apenas para administradores.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpGet("usuarios/{usuarioId:int}")]
        [ProducesResponseType(typeof(HoleriteViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHoleritesUsuario(int usuarioId)
        {
            try
            {
                return Ok(await _holeriteService.GetHoleritesUsuarioAsync(usuarioId, User));
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
                _logger.LogError(ex, "Erro ao listar holerites do usuario {UsuarioId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Envia um holerite em PDF para um professor ou administrador. Disponivel apenas para administradores.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpPost("usuarios/{usuarioId:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(HoleriteViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadHolerite(
            int usuarioId,
            [FromForm] HoleriteUploadViewModel model)
        {
            try
            {
                if (model.Arquivo == null)
                {
                    return BadRequest("Informe o arquivo do holerite.");
                }

                var created = await _holeriteService.UploadHoleriteAsync(
                    usuarioId,
                    model.CompetenciaMes,
                    model.CompetenciaAno,
                    model.Arquivo,
                    User);

                return CreatedAtAction(nameof(GetHoleritesUsuario), new { usuarioId }, created);
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
                _logger.LogError(ex, "Erro ao enviar holerite do usuario {UsuarioId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Baixa holerite de um funcionario. Disponivel apenas para administradores.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpGet("usuarios/{usuarioId:int}/{holeriteId:int}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadHoleriteUsuario(int usuarioId, int holeriteId)
        {
            try
            {
                var arquivo = await _holeriteService.DownloadHoleriteUsuarioAsync(usuarioId, holeriteId, User);
                if (arquivo == null) return NotFound();

                return File(arquivo.Stream, arquivo.ContentType, arquivo.NomeArquivo);
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
                _logger.LogError(ex, "Erro ao baixar holerite {HoleriteId} do usuario {UsuarioId}", holeriteId, usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Gera um link temporario para compartilhar holerite de um funcionario. Disponivel apenas para administradores.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpPost("usuarios/{usuarioId:int}/{holeriteId:int}/compartilhamento")]
        [ProducesResponseType(typeof(HoleriteCompartilhamentoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompartilharHoleriteUsuario(int usuarioId, int holeriteId)
        {
            try
            {
                var compartilhamento = await _holeriteService.CriarCompartilhamentoHoleriteUsuarioAsync(usuarioId, holeriteId, User);
                if (compartilhamento == null) return NotFound();

                compartilhamento.Url = CriarUrlCompartilhamento(compartilhamento.Token);
                return Ok(compartilhamento);
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
                _logger.LogError(ex, "Erro ao compartilhar holerite {HoleriteId} do usuario {UsuarioId}", holeriteId, usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Baixa um holerite por link temporario de compartilhamento.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("compartilhados/{token}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadHoleriteCompartilhado(string token)
        {
            try
            {
                var arquivo = await _holeriteService.DownloadHoleriteCompartilhadoAsync(token);
                if (arquivo == null) return NotFound();

                Response.Headers[HeaderNames.ContentDisposition] =
                    new ContentDispositionHeaderValue("inline")
                    {
                        FileNameStar = arquivo.NomeArquivo
                    }.ToString();

                return File(arquivo.Stream, arquivo.ContentType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao baixar holerite compartilhado");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Remove holerite de um funcionario. Disponivel apenas para administradores.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpDelete("usuarios/{usuarioId:int}/{holeriteId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteHolerite(int usuarioId, int holeriteId)
        {
            try
            {
                var deleted = await _holeriteService.DeleteHoleriteAsync(usuarioId, holeriteId, User);
                return deleted ? Ok() : NotFound();
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
                _logger.LogError(ex, "Erro ao excluir holerite {HoleriteId} do usuario {UsuarioId}", holeriteId, usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

    private string CriarUrlCompartilhamento(string token)
    {
        return Url.Action(
            nameof(DownloadHoleriteCompartilhado),
            null,
            new { token },
            Request.Scheme,
            Request.Host.Value) ?? string.Empty;
    }
}
}

using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace ESCOLA_API.Controllers
{
    /// <summary>
    /// Consulta, fluxo de liberacao e compartilhamento do boletim escolar digital.
    /// </summary>
    [Authorize]
    [Route("api/boletins-digitais")]
    [ApiController]
    [Produces("application/json")]
    public class BoletinsDigitaisController : ControllerBase
    {
        private readonly IBoletimDigitalService _boletimDigitalService;
        private readonly ILogger<BoletinsDigitaisController> _logger;

        public BoletinsDigitaisController(
            IBoletimDigitalService boletimDigitalService,
            ILogger<BoletinsDigitaisController> logger)
        {
            _boletimDigitalService = boletimDigitalService;
            _logger = logger;
        }

        /// <summary>
        /// Consulta o boletim do aluno logado, mesmo com disciplinas pendentes de lancamento.
        /// </summary>
        [Authorize(Roles = "Aluno")]
        [HttpGet("me")]
        [ProducesResponseType(typeof(BoletimDigitalViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMeuBoletim()
        {
            try
            {
                return Ok(await _boletimDigitalService.GetMeuBoletimAsync(User));
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
                _logger.LogError(ex, "Erro ao consultar boletim do aluno logado");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Consulta o boletim de um aluno. Disponivel para administradores e professores autorizados.
        /// </summary>
        [Authorize(Roles = "Administrador,Professor")]
        [HttpGet("alunos/{alunoUsuarioId:int}")]
        [ProducesResponseType(typeof(BoletimDigitalViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBoletimAluno(int alunoUsuarioId)
        {
            try
            {
                var boletim = await _boletimDigitalService.GetBoletimAlunoAsync(alunoUsuarioId, User);
                return boletim == null ? NotFound() : Ok(boletim);
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
                _logger.LogError(ex, "Erro ao consultar boletim do aluno {AlunoUsuarioId}", alunoUsuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Lista boletins completos que o professor enviou para liberacao da Diretoria.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpGet("pendentes-liberacao")]
        [ProducesResponseType(typeof(BoletimDigitalResumoAlunoViewModel[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPendentesLiberacao()
        {
            try
            {
                return Ok(await _boletimDigitalService.GetPendentesLiberacaoAsync(User));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar boletins pendentes de liberacao");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Professor envia um boletim completo para a Diretoria liberar.
        /// </summary>
        [Authorize(Roles = "Professor")]
        [HttpPost("alunos/{alunoUsuarioId:int}/solicitar-liberacao")]
        [ProducesResponseType(typeof(BoletimDigitalViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SolicitarLiberacao(int alunoUsuarioId)
        {
            try
            {
                var boletim = await _boletimDigitalService.SolicitarLiberacaoAsync(alunoUsuarioId, User);
                return boletim == null ? NotFound() : Ok(boletim);
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
                _logger.LogError(ex, "Erro ao solicitar liberacao do boletim do aluno {AlunoUsuarioId}", alunoUsuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Diretoria libera um boletim completo para o aluno.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpPost("alunos/{alunoUsuarioId:int}/liberar")]
        [ProducesResponseType(typeof(BoletimDigitalViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Liberar(int alunoUsuarioId)
        {
            try
            {
                var boletim = await _boletimDigitalService.LiberarAsync(alunoUsuarioId, User);
                return boletim == null ? NotFound() : Ok(boletim);
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
                _logger.LogError(ex, "Erro ao liberar boletim do aluno {AlunoUsuarioId}", alunoUsuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Baixa o PDF do boletim do aluno logado. Exige boletim completo e liberado.
        /// </summary>
        [Authorize(Roles = "Aluno")]
        [HttpGet("me/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadMeuBoletimPdf()
        {
            try
            {
                var arquivo = await _boletimDigitalService.DownloadMeuBoletimPdfAsync(User);
                return arquivo == null ? NotFound() : File(arquivo.Stream, arquivo.ContentType, arquivo.NomeArquivo);
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
                _logger.LogError(ex, "Erro ao baixar PDF do boletim do aluno logado");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Baixa o PDF do boletim de um aluno. Exige boletim completo e liberado.
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpGet("alunos/{alunoUsuarioId:int}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadBoletimAlunoPdf(int alunoUsuarioId)
        {
            try
            {
                var arquivo = await _boletimDigitalService.DownloadBoletimAlunoPdfAsync(alunoUsuarioId, User);
                return arquivo == null ? NotFound() : File(arquivo.Stream, arquivo.ContentType, arquivo.NomeArquivo);
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
                _logger.LogError(ex, "Erro ao baixar PDF do boletim do aluno {AlunoUsuarioId}", alunoUsuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [Authorize(Roles = "Aluno")]
        [HttpPost("me/compartilhamento")]
        [HttpPost("me/whatsapp")]
        [HttpPost("me/email")]
        [ProducesResponseType(typeof(BoletimDigitalCompartilhamentoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompartilharMeuBoletim()
        {
            try
            {
                var compartilhamento = await _boletimDigitalService.CriarCompartilhamentoMeuBoletimAsync(User);
                return compartilhamento == null
                    ? NotFound()
                    : Ok(CompletarCompartilhamento(compartilhamento));
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
                _logger.LogError(ex, "Erro ao compartilhar boletim do aluno logado");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("alunos/{alunoUsuarioId:int}/compartilhamento")]
        [HttpPost("alunos/{alunoUsuarioId:int}/whatsapp")]
        [HttpPost("alunos/{alunoUsuarioId:int}/email")]
        [ProducesResponseType(typeof(BoletimDigitalCompartilhamentoViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompartilharBoletimAluno(int alunoUsuarioId)
        {
            try
            {
                var compartilhamento = await _boletimDigitalService.CriarCompartilhamentoBoletimAlunoAsync(alunoUsuarioId, User);
                return compartilhamento == null
                    ? NotFound()
                    : Ok(CompletarCompartilhamento(compartilhamento));
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
                _logger.LogError(ex, "Erro ao compartilhar boletim do aluno {AlunoUsuarioId}", alunoUsuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        /// <summary>
        /// Baixa o PDF do boletim por link temporario publico.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("compartilhados/{token}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadBoletimCompartilhado(string token)
        {
            try
            {
                var arquivo = await _boletimDigitalService.DownloadBoletimCompartilhadoAsync(token);
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
                _logger.LogError(ex, "Erro ao baixar boletim compartilhado");
                return StatusCode(StatusCodes.Status500InternalServerError, "Banco de Dados Falhou");
            }
        }

        private BoletimDigitalCompartilhamentoViewModel CompletarCompartilhamento(
            BoletimDigitalCompartilhamentoViewModel compartilhamento)
        {
            compartilhamento.Url = CriarUrlCompartilhamento(compartilhamento.Token);
            compartilhamento.Texto = $"{compartilhamento.Texto}\nLink: {compartilhamento.Url}";
            compartilhamento.EmailCompartilhamentoUrl = CriarEmailUrl(compartilhamento.Texto);
            compartilhamento.WhatsAppCompartilhamentoUrl = CriarWhatsAppUrl(compartilhamento.Texto);
            return compartilhamento;
        }

        private string CriarUrlCompartilhamento(string token)
        {
            return Url.Action(
                nameof(DownloadBoletimCompartilhado),
                null,
                new { token },
                Request.Scheme,
                Request.Host.Value) ?? string.Empty;
        }

        private static string CriarEmailUrl(string texto)
        {
            return $"mailto:?subject={Uri.EscapeDataString("Boletim escolar digital")}&body={Uri.EscapeDataString(texto)}";
        }

        private static string CriarWhatsAppUrl(string texto)
        {
            return $"https://wa.me/?text={Uri.EscapeDataString(texto)}";
        }
    }
}

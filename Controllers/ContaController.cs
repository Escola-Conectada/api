using System.Net.Mail;
using System.Net;
using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESCOLA_API.Controllers
{
    /// <summary>
    /// Fluxos publicos relacionados a conta do usuario.
    /// </summary>
    [AllowAnonymous]
    [Route("conta")]
    public class ContaController : Controller
    {
        private readonly ILogger<ContaController> _logger;
        private readonly IUsuarioService _usuarioService;
        private readonly IConfiguracaoAplicacaoService _configuracaoAplicacaoService;

        public ContaController(
            IUsuarioService usuarioService,
            IConfiguracaoAplicacaoService configuracaoAplicacaoService,
            ILogger<ContaController> logger)
        {
            _usuarioService = usuarioService;
            _configuracaoAplicacaoService = configuracaoAplicacaoService;
            _logger = logger;
        }

        /// <summary>
        /// Recebe solicitacoes publicas de exclusao de conta fora do app.
        /// </summary>
        [HttpPost("exclusao")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> SolicitarExclusaoConta([FromForm] SolicitarExclusaoContaPublicaViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || !MailAddress.TryCreate(model.Email, out _))
            {
                return await HtmlResponseAsync(
                    "Solicitacao nao enviada",
                    "Informe um email valido para solicitar a exclusao da conta.",
                    StatusCodes.Status400BadRequest);
            }

            var solicitacao = await _usuarioService.SolicitarExclusaoContaPorEmailAsync(model);
            if (solicitacao == null)
            {
                _logger.LogWarning("Solicitacao publica de exclusao recebida para email nao cadastrado.");
            }
            else
            {
                _logger.LogInformation(
                    "Solicitacao publica de exclusao registrada para usuario {UsuarioId}",
                    solicitacao.IdUsuario);
            }

            return await HtmlResponseAsync(
                "Solicitacao recebida",
                "Se o email informado estiver cadastrado, a solicitacao sera analisada pela administracao.");
        }

        private async Task<ContentResult> HtmlResponseAsync(
            string title,
            string message,
            int statusCode = StatusCodes.Status200OK)
        {
            var nomeEscola = WebUtility.HtmlEncode(await _configuracaoAplicacaoService.GetNomeEscolaAsync());

            return new ContentResult
            {
                Content = HtmlPage(title, message, nomeEscola),
                ContentType = "text/html; charset=utf-8",
                StatusCode = statusCode
            };
        }

        private static string HtmlPage(string title, string message, string nomeEscola)
        {
            return $$"""
                <!doctype html>
                <html lang="pt-BR">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1">
                    <title>{{title}} - {{nomeEscola}}</title>
                    <style>
                        body { font-family: Arial, sans-serif; margin: 0; padding: 32px; line-height: 1.5; color: #1f2937; }
                        main { max-width: 680px; margin: 0 auto; }
                        a { color: #2563eb; }
                    </style>
                </head>
                <body>
                    <main>
                        <h1>{{title}}</h1>
                        <p>{{message}}</p>
                        <p><a href="/legal/exclusao-conta">Voltar</a></p>
                    </main>
                </body>
                </html>
                """;
        }
    }
}

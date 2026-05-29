using System.Net.Mail;
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

        public ContaController(IUsuarioService usuarioService, ILogger<ContaController> logger)
        {
            _usuarioService = usuarioService;
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
                return HtmlResponse(
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

            return HtmlResponse(
                "Solicitacao recebida",
                "Se o email informado estiver cadastrado, a solicitacao sera analisada pela administracao.");
        }

        private static ContentResult HtmlResponse(string title, string message, int statusCode = StatusCodes.Status200OK)
        {
            return new ContentResult
            {
                Content = HtmlPage(title, message),
                ContentType = "text/html; charset=utf-8",
                StatusCode = statusCode
            };
        }

        private static string HtmlPage(string title, string message)
        {
            return $$"""
                <!doctype html>
                <html lang="pt-BR">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1">
                    <title>{{title}} - Escola Conectada</title>
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

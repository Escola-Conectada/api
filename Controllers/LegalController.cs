using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESCOLA_API.Controllers
{
    /// <summary>
    /// Paginas publicas exigidas por lojas de aplicativos.
    /// </summary>
    [AllowAnonymous]
    [Route("legal")]
    public class LegalController : Controller
    {
        private readonly IConfiguration _configuration;

        public LegalController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("privacidade")]
        public IActionResult Privacidade()
        {
            var appName = AppName();
            var supportEmail = SupportEmail();

            return HtmlPage(
                "Politica de Privacidade",
                $$"""
                <p>O {{appName}} usa os dados cadastrais necessarios para identificacao escolar, autenticacao, comunicacao, registros academicos, arquivos vinculados ao usuario, notificacoes e recursos administrativos.</p>
                <p>Os dados podem incluir nome, email, telefone, data de nascimento, nome dos responsaveis, endereco, perfil de acesso, foto, certificados, holerites, notificacoes, caderneta digital, calendario escolar e registros tecnicos de seguranca.</p>
                <p>Os dados sao usados para entregar as funcionalidades do sistema escolar e nao devem ser vendidos. Arquivos podem ser armazenados em provedor de nuvem configurado pela instituicao.</p>
                <p>Para suporte, correcao de dados ou solicitacao de exclusao de conta, entre em contato por <a href="mailto:{{supportEmail}}">{{supportEmail}}</a> ou use a pagina <a href="/legal/exclusao-conta">Exclusao de conta</a>.</p>
                <p>Alguns dados podem precisar ser mantidos pelo periodo necessario para obrigacoes legais, auditoria, seguranca, prevencao de fraude ou registros escolares.</p>
                """);
        }

        [HttpGet("suporte")]
        public IActionResult Suporte()
        {
            var appName = AppName();
            var supportEmail = SupportEmail();

            return HtmlPage(
                "Suporte",
                $$"""
                <p>Para ajuda com acesso, senha, dados cadastrais, notificacoes, documentos ou exclusao de conta do {{appName}}, envie uma mensagem para <a href="mailto:{{supportEmail}}">{{supportEmail}}</a>.</p>
                <p>Ao entrar em contato, informe seu nome completo e o email cadastrado. Nao envie sua senha.</p>
                """);
        }

        [HttpGet("exclusao-conta")]
        public IActionResult ExclusaoConta()
        {
            var appName = AppName();
            var supportEmail = SupportEmail();

            return HtmlPage(
                "Exclusao de conta",
                $$"""
                <p>Usuarios do {{appName}} podem solicitar a exclusao da conta pelo app em Perfil &gt; Exclusao de conta ou por esta pagina.</p>
                <form method="post" action="/conta/exclusao">
                    <label for="email">Email cadastrado</label>
                    <input id="email" name="Email" type="email" maxlength="150" required>
                    <label for="motivo">Motivo opcional</label>
                    <textarea id="motivo" name="Motivo" maxlength="500" rows="4"></textarea>
                    <button type="submit">Solicitar exclusao</button>
                </form>
                <p>A solicitacao sera analisada pela administracao. Dados que precisem ser mantidos por obrigacao legal, auditoria, seguranca ou registros escolares poderao ser retidos conforme a politica de privacidade.</p>
                <p>Suporte: <a href="mailto:{{supportEmail}}">{{supportEmail}}</a></p>
                """);
        }

        private ContentResult HtmlPage(string title, string body)
        {
            var appName = AppName();
            var safeTitle = WebUtility.HtmlEncode(title);

            var html = $$"""
                <!doctype html>
                <html lang="pt-BR">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1">
                    <title>{{safeTitle}} - {{appName}}</title>
                    <style>
                        body { font-family: Arial, sans-serif; margin: 0; padding: 32px; line-height: 1.5; color: #1f2937; background: #f9fafb; }
                        main { max-width: 760px; margin: 0 auto; background: #fff; padding: 28px; border: 1px solid #e5e7eb; border-radius: 8px; }
                        h1 { margin-top: 0; font-size: 28px; }
                        label { display: block; margin: 16px 0 6px; font-weight: 700; }
                        input, textarea { box-sizing: border-box; width: 100%; padding: 10px; border: 1px solid #cbd5e1; border-radius: 6px; font: inherit; }
                        button { margin-top: 16px; padding: 10px 16px; border: 0; border-radius: 6px; background: #2563eb; color: #fff; font: inherit; cursor: pointer; }
                        a { color: #2563eb; }
                    </style>
                </head>
                <body>
                    <main>
                        <h1>{{safeTitle}}</h1>
                        {{body}}
                    </main>
                </body>
                </html>
                """;

            return Content(html, "text/html; charset=utf-8");
        }

        private string AppName()
        {
            return WebUtility.HtmlEncode(_configuration["Legal:AppName"] ?? "Escola Conectada");
        }

        private string SupportEmail()
        {
            return WebUtility.HtmlEncode(_configuration["Legal:SupportEmail"] ?? "suporte@escola.com");
        }
    }
}

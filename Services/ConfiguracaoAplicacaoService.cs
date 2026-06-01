using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public class ConfiguracaoAplicacaoService : IConfiguracaoAplicacaoService
    {
        public const int ConfiguracaoPadraoId = 1;
        public const string NomeEscolaPadrao = "Escola Conectada";

        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public ConfiguracaoAplicacaoService(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ConfiguracaoAplicacaoViewModel> GetAsync(CancellationToken cancellationToken = default)
        {
            var configuracao = await _context.ConfiguracoesAplicacao
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.IdConfiguracaoAplicacao == ConfiguracaoPadraoId, cancellationToken);

            return configuracao == null
                ? new ConfiguracaoAplicacaoViewModel
                {
                    NomeEscola = ResolveNomeEscolaPadrao(),
                    AtualizadoEmUtc = DateTime.UnixEpoch
                }
                : ToViewModel(configuracao);
        }

        public async Task<string> GetNomeEscolaAsync(CancellationToken cancellationToken = default)
        {
            var configuracao = await GetAsync(cancellationToken);
            return configuracao.NomeEscola;
        }

        public async Task<ConfiguracaoAplicacaoViewModel> UpdateAsync(
            ConfiguracaoAplicacaoUpdateViewModel viewModel,
            ClaimsPrincipal principal,
            CancellationToken cancellationToken = default)
        {
            if (!principal.IsInRole(PerfilSistema.Administrador))
            {
                throw new UnauthorizedAccessException("Apenas administradores podem alterar o nome da escola.");
            }

            var nomeEscola = NormalizarNomeEscola(viewModel.NomeEscola);
            if (string.IsNullOrWhiteSpace(nomeEscola))
            {
                throw new InvalidOperationException("Informe o nome da escola.");
            }

            if (nomeEscola.Length > 120)
            {
                throw new InvalidOperationException("O nome da escola deve ter ate 120 caracteres.");
            }

            var configuracao = await _context.ConfiguracoesAplicacao
                .FirstOrDefaultAsync(item => item.IdConfiguracaoAplicacao == ConfiguracaoPadraoId, cancellationToken);

            if (configuracao == null)
            {
                configuracao = new ConfiguracaoAplicacao
                {
                    IdConfiguracaoAplicacao = ConfiguracaoPadraoId
                };
                _context.ConfiguracoesAplicacao.Add(configuracao);
            }

            configuracao.NomeEscola = nomeEscola;
            configuracao.AtualizadoEmUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return ToViewModel(configuracao);
        }

        private string ResolveNomeEscolaPadrao()
        {
            return NormalizarNomeEscola(_configuration["Legal:AppName"]) ?? NomeEscolaPadrao;
        }

        private static ConfiguracaoAplicacaoViewModel ToViewModel(ConfiguracaoAplicacao configuracao)
        {
            return new ConfiguracaoAplicacaoViewModel
            {
                NomeEscola = NormalizarNomeEscola(configuracao.NomeEscola) ?? NomeEscolaPadrao,
                AtualizadoEmUtc = configuracao.AtualizadoEmUtc
            };
        }

        private static string? NormalizarNomeEscola(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}

using System.Security.Claims;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public interface IConfiguracaoAplicacaoService
    {
        Task<ConfiguracaoAplicacaoViewModel> GetAsync(CancellationToken cancellationToken = default);
        Task<string> GetNomeEscolaAsync(CancellationToken cancellationToken = default);
        Task<ConfiguracaoAplicacaoViewModel> UpdateAsync(
            ConfiguracaoAplicacaoUpdateViewModel viewModel,
            ClaimsPrincipal principal,
            CancellationToken cancellationToken = default);
    }
}

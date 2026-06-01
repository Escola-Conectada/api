using System.Security.Claims;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public interface IBoletimDigitalService
    {
        Task<BoletimDigitalViewModel> GetMeuBoletimAsync(ClaimsPrincipal principal);
        Task<BoletimDigitalViewModel?> GetBoletimAlunoAsync(int alunoUsuarioId, ClaimsPrincipal principal);
        Task<BoletimDigitalResumoAlunoViewModel[]> GetPendentesLiberacaoAsync(ClaimsPrincipal principal);
        Task<BoletimDigitalViewModel?> SolicitarLiberacaoAsync(int alunoUsuarioId, ClaimsPrincipal principal);
        Task<BoletimDigitalViewModel?> LiberarAsync(int alunoUsuarioId, ClaimsPrincipal principal);
        Task<ArquivoDownload?> DownloadMeuBoletimPdfAsync(ClaimsPrincipal principal);
        Task<ArquivoDownload?> DownloadBoletimAlunoPdfAsync(int alunoUsuarioId, ClaimsPrincipal principal);
        Task<ArquivoDownload?> DownloadBoletimCompartilhadoAsync(string token);
        Task<BoletimDigitalCompartilhamentoViewModel?> CriarCompartilhamentoMeuBoletimAsync(ClaimsPrincipal principal);
        Task<BoletimDigitalCompartilhamentoViewModel?> CriarCompartilhamentoBoletimAlunoAsync(int alunoUsuarioId, ClaimsPrincipal principal);
    }
}

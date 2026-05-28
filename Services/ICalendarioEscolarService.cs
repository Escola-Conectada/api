using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public interface ICalendarioEscolarService
    {
        CalendarioEscolarAnoViewModel GetCalendarioAnual(int? ano, int? mesSelecionado);
    }
}

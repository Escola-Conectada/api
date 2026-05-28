using System.Globalization;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public class CalendarioEscolarService : ICalendarioEscolarService
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

        public CalendarioEscolarAnoViewModel GetCalendarioAnual(int? ano, int? mesSelecionado)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var anoCalendario = ano ?? hoje.Year;
            var mesAtual = mesSelecionado ?? (anoCalendario == hoje.Year ? hoje.Month : 1);

            if (anoCalendario < 1900 || anoCalendario > 2100)
            {
                throw new InvalidOperationException("Informe um ano entre 1900 e 2100.");
            }

            if (mesAtual < 1 || mesAtual > 12)
            {
                throw new InvalidOperationException("Informe um mes entre 1 e 12.");
            }

            var feriados = GetFeriadosNacionais(anoCalendario)
                .OrderBy(feriado => feriado.Data)
                .ToArray();
            var feriadosPorData = feriados.ToDictionary(feriado => feriado.Data);

            return new CalendarioEscolarAnoViewModel
            {
                Ano = anoCalendario,
                MesSelecionado = mesAtual,
                FeriadosNacionais = feriados,
                Meses = Enumerable.Range(1, 12)
                    .Select(mes => CriarMes(anoCalendario, mes, feriadosPorData))
                    .ToArray()
            };
        }

        private static CalendarioEscolarMesViewModel CriarMes(
            int ano,
            int mes,
            IReadOnlyDictionary<DateOnly, FeriadoNacionalViewModel> feriadosPorData)
        {
            var diasNoMes = DateTime.DaysInMonth(ano, mes);

            return new CalendarioEscolarMesViewModel
            {
                Mes = mes,
                NomeMes = PtBr.DateTimeFormat.GetMonthName(mes),
                Dias = Enumerable.Range(1, diasNoMes)
                    .Select(dia => CriarDia(new DateOnly(ano, mes, dia), feriadosPorData))
                    .ToArray()
            };
        }

        private static CalendarioEscolarDiaViewModel CriarDia(
            DateOnly data,
            IReadOnlyDictionary<DateOnly, FeriadoNacionalViewModel> feriadosPorData)
        {
            feriadosPorData.TryGetValue(data, out var feriado);

            return new CalendarioEscolarDiaViewModel
            {
                Data = data,
                Dia = data.Day,
                DiaSemana = PtBr.DateTimeFormat.GetDayName(data.DayOfWeek),
                FinalDeSemana = data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
                FeriadoNacional = feriado != null,
                NomeFeriado = feriado?.Nome
            };
        }

        private static FeriadoNacionalViewModel[] GetFeriadosNacionais(int ano)
        {
            var pascoa = CalcularDomingoDePascoa(ano);
            var paixaoDeCristo = pascoa.AddDays(-2);

            return
            [
                CriarFeriado(new DateOnly(ano, 1, 1), "Confraternizacao Universal"),
                CriarFeriado(paixaoDeCristo, "Paixao de Cristo"),
                CriarFeriado(new DateOnly(ano, 4, 21), "Tiradentes"),
                CriarFeriado(new DateOnly(ano, 5, 1), "Dia Mundial do Trabalho"),
                CriarFeriado(new DateOnly(ano, 9, 7), "Independencia do Brasil"),
                CriarFeriado(new DateOnly(ano, 10, 12), "Nossa Senhora Aparecida"),
                CriarFeriado(new DateOnly(ano, 11, 2), "Finados"),
                CriarFeriado(new DateOnly(ano, 11, 15), "Proclamacao da Republica"),
                CriarFeriado(new DateOnly(ano, 11, 20), "Dia Nacional de Zumbi e da Consciencia Negra"),
                CriarFeriado(new DateOnly(ano, 12, 25), "Natal")
            ];
        }

        private static FeriadoNacionalViewModel CriarFeriado(DateOnly data, string nome)
        {
            return new FeriadoNacionalViewModel
            {
                Data = data,
                Nome = nome
            };
        }

        private static DateOnly CalcularDomingoDePascoa(int ano)
        {
            var a = ano % 19;
            var b = ano / 100;
            var c = ano % 100;
            var d = b / 4;
            var e = b % 4;
            var f = (b + 8) / 25;
            var g = (b - f + 1) / 3;
            var h = (19 * a + b - d - g + 15) % 30;
            var i = c / 4;
            var k = c % 4;
            var l = (32 + 2 * e + 2 * i - h - k) % 7;
            var m = (a + 11 * h + 22 * l) / 451;
            var mes = (h + l - 7 * m + 114) / 31;
            var dia = ((h + l - 7 * m + 114) % 31) + 1;

            return new DateOnly(ano, mes, dia);
        }
    }
}

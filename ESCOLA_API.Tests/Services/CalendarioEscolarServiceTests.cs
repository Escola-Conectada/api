using ESCOLA_API.Services;

namespace ESCOLA_API.Tests.Services
{
    public class CalendarioEscolarServiceTests
    {
        [Fact]
        public void GetCalendarioAnual_WhenAnoIs2026_ReturnsNationalHolidaysAndSelectedMonth()
        {
            var service = new CalendarioEscolarService();

            var calendario = service.GetCalendarioAnual(2026, 5);

            Assert.Equal(2026, calendario.Ano);
            Assert.Equal(5, calendario.MesSelecionado);
            Assert.Equal(12, calendario.Meses.Length);
            Assert.Equal(10, calendario.FeriadosNacionais.Length);
            Assert.Contains(calendario.FeriadosNacionais, feriado =>
                feriado.Data == new DateOnly(2026, 4, 3)
                && feriado.Nome == "Paixao de Cristo");
            Assert.Contains(calendario.FeriadosNacionais, feriado =>
                feriado.Data == new DateOnly(2026, 11, 20)
                && feriado.Nome.Contains("Consciencia Negra"));

            var novembro20 = calendario.Meses[10].Dias.Single(dia => dia.Data == new DateOnly(2026, 11, 20));
            Assert.True(novembro20.FeriadoNacional);
            Assert.Equal("Dia Nacional de Zumbi e da Consciencia Negra", novembro20.NomeFeriado);
        }

        [Fact]
        public void GetCalendarioAnual_WhenMesIsInvalid_ThrowsInvalidOperationException()
        {
            var service = new CalendarioEscolarService();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                service.GetCalendarioAnual(2026, 13));

            Assert.Equal("Informe um mes entre 1 e 12.", exception.Message);
        }
    }
}

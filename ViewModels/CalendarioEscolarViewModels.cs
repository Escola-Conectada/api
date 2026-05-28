namespace ESCOLA_API.ViewModels
{
    public class CalendarioEscolarAnoViewModel
    {
        public int Ano { get; set; }
        public int MesSelecionado { get; set; }
        public CalendarioEscolarMesViewModel[] Meses { get; set; } = [];
        public FeriadoNacionalViewModel[] FeriadosNacionais { get; set; } = [];
    }

    public class CalendarioEscolarMesViewModel
    {
        public int Mes { get; set; }
        public string NomeMes { get; set; } = string.Empty;
        public CalendarioEscolarDiaViewModel[] Dias { get; set; } = [];
    }

    public class CalendarioEscolarDiaViewModel
    {
        public DateOnly Data { get; set; }
        public int Dia { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public bool FinalDeSemana { get; set; }
        public bool FeriadoNacional { get; set; }
        public string? NomeFeriado { get; set; }
    }

    public class FeriadoNacionalViewModel
    {
        public DateOnly Data { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Feriado Nacional";
    }
}

using System.Globalization;
using System.Security.Claims;
using System.Text;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public class CalendarioEscolarService : ICalendarioEscolarService
    {
        private const string PublicoTodos = "Todos";
        private const string PublicoProfessores = "Professores";
        private const string PublicoAlunos = "Alunos";
        private const string PublicoAdministradores = "Administradores";
        private const string PublicoAlunosEProfessores = "AlunosEProfessores";
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
        private readonly DataContext _context;
        private readonly IConfiguracaoAplicacaoService? _configuracaoAplicacaoService;

        public CalendarioEscolarService(
            DataContext context,
            IConfiguracaoAplicacaoService? configuracaoAplicacaoService = null)
        {
            _context = context;
            _configuracaoAplicacaoService = configuracaoAplicacaoService;
        }

        public async Task<CalendarioEscolarAnoViewModel> GetCalendarioAnualAsync(int? ano, int? mesSelecionado)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var anoCalendario = ano ?? hoje.Year;
            var mesAtual = mesSelecionado ?? (anoCalendario == hoje.Year ? hoje.Month : 1);

            ValidarPeriodo(anoCalendario, mesAtual);

            var feriados = GetFeriadosNacionais(anoCalendario)
                .OrderBy(feriado => feriado.Data)
                .ToArray();
            var feriadosPorData = feriados.ToDictionary(feriado => feriado.Data);
            var eventos = await _context.CalendarioEscolarEventos
                .AsNoTracking()
                .Where(evento => evento.Data.Year == anoCalendario)
                .OrderBy(evento => evento.Data)
                .ThenBy(evento => evento.Titulo)
                .ToArrayAsync();
            var eventosView = eventos.Select(evento => ToViewModel(evento)).ToArray();
            var eventosPorData = eventosView
                .GroupBy(evento => evento.Data)
                .ToDictionary(group => group.Key, group => group.ToArray());

            return new CalendarioEscolarAnoViewModel
            {
                Ano = anoCalendario,
                MesSelecionado = mesAtual,
                FeriadosNacionais = feriados,
                Eventos = eventosView,
                Meses = Enumerable.Range(1, 12)
                    .Select(mes => CriarMes(anoCalendario, mes, feriadosPorData, eventosPorData))
                    .ToArray()
            };
        }

        public async Task<CalendarioEscolarEventoViewModel> AddEventoAsync(
            CalendarioEscolarEventoCreateViewModel viewModel,
            ClaimsPrincipal principal)
        {
            if (!principal.IsInRole(PerfilSistema.Administrador))
            {
                throw new UnauthorizedAccessException("Apenas administradores podem cadastrar eventos no calendario escolar.");
            }

            ValidarEvento(viewModel);

            var idUsuarioCriador = GetUsuarioAtualId(principal);
            if (idUsuarioCriador <= 0)
            {
                throw new UnauthorizedAccessException("Sessao invalida para cadastrar evento no calendario escolar.");
            }

            var nomeCriador = await _context.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.IdUsuario == idUsuarioCriador)
                .Select(usuario => usuario.Nome)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(nomeCriador))
            {
                throw new UnauthorizedAccessException("Usuario administrador nao encontrado.");
            }

            var tipo = NormalizarTexto(viewModel.Tipo, "Evento");
            var publicoAlvo = ResolverPublicoAlvo(viewModel.PublicoAlvo, tipo);
            var perfisDestinatarios = ResolverPerfisPublico(publicoAlvo);
            var evento = new CalendarioEscolarEvento
            {
                Data = viewModel.Data,
                Tipo = tipo,
                Titulo = viewModel.Titulo.Trim(),
                Descricao = NormalizarDescricao(viewModel.Descricao),
                PublicoAlvo = publicoAlvo,
                IdUsuarioCriador = idUsuarioCriador,
                NomeUsuarioCriador = nomeCriador,
                CriadoEmUtc = DateTime.UtcNow
            };

            _context.CalendarioEscolarEventos.Add(evento);
            await _context.SaveChangesAsync();

            var totalNotificados = await CriarNotificacoesEventoAsync(evento, perfisDestinatarios);
            return ToViewModel(evento, perfisDestinatarios, totalNotificados);
        }

        private static CalendarioEscolarMesViewModel CriarMes(
            int ano,
            int mes,
            IReadOnlyDictionary<DateOnly, FeriadoNacionalViewModel> feriadosPorData,
            IReadOnlyDictionary<DateOnly, CalendarioEscolarEventoViewModel[]> eventosPorData)
        {
            var diasNoMes = DateTime.DaysInMonth(ano, mes);

            return new CalendarioEscolarMesViewModel
            {
                Mes = mes,
                NomeMes = PtBr.DateTimeFormat.GetMonthName(mes),
                Dias = Enumerable.Range(1, diasNoMes)
                    .Select(dia => CriarDia(new DateOnly(ano, mes, dia), feriadosPorData, eventosPorData))
                    .ToArray()
            };
        }

        private static CalendarioEscolarDiaViewModel CriarDia(
            DateOnly data,
            IReadOnlyDictionary<DateOnly, FeriadoNacionalViewModel> feriadosPorData,
            IReadOnlyDictionary<DateOnly, CalendarioEscolarEventoViewModel[]> eventosPorData)
        {
            feriadosPorData.TryGetValue(data, out var feriado);
            eventosPorData.TryGetValue(data, out var eventos);

            return new CalendarioEscolarDiaViewModel
            {
                Data = data,
                Dia = data.Day,
                DiaSemana = PtBr.DateTimeFormat.GetDayName(data.DayOfWeek),
                FinalDeSemana = data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
                FeriadoNacional = feriado != null,
                NomeFeriado = feriado?.Nome,
                Eventos = eventos ?? []
            };
        }

        private async Task<int> CriarNotificacoesEventoAsync(
            CalendarioEscolarEvento evento,
            int[] perfisDestinatarios)
        {
            var usuariosIds = await _context.Usuarios
                .AsNoTracking()
                .Where(usuario => perfisDestinatarios.Contains(usuario.IdPerfil))
                .Select(usuario => usuario.IdUsuario)
                .Distinct()
                .ToArrayAsync();

            if (usuariosIds.Length == 0)
            {
                return 0;
            }

            var data = evento.Data.ToString("dd/MM/yyyy", PtBr);
            var descricao = string.IsNullOrWhiteSpace(evento.Descricao)
                ? string.Empty
                : $" Descricao: {evento.Descricao}.";
            var nomeEscola = await GetNomeEscolaAsync();
            var mensagem = $"Evento cadastrado no calendario escolar de {nomeEscola} por {evento.NomeUsuarioCriador}. "
                + $"Data: {data}. Tipo: {evento.Tipo}. Titulo: {evento.Titulo}.{descricao}";
            var dataIso = evento.Data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var notificacoes = usuariosIds.Select(idUsuario => new Notificacao
            {
                IdUsuario = idUsuario,
                Tipo = "CalendarioEscolarEvento",
                Titulo = evento.Titulo,
                Mensagem = mensagem,
                Link = $"/calendario-escolar?data={dataIso}&eventoId={evento.IdEventoCalendarioEscolar}",
                CriadaEmUtc = DateTime.UtcNow
            });

            _context.Notificacoes.AddRange(notificacoes);
            await _context.SaveChangesAsync();
            return usuariosIds.Length;
        }

        private async Task<string> GetNomeEscolaAsync()
        {
            return _configuracaoAplicacaoService == null
                ? ConfiguracaoAplicacaoService.NomeEscolaPadrao
                : await _configuracaoAplicacaoService.GetNomeEscolaAsync();
        }

        private static CalendarioEscolarEventoViewModel ToViewModel(
            CalendarioEscolarEvento evento,
            int[]? perfisDestinatarios = null,
            int totalNotificados = 0)
        {
            perfisDestinatarios ??= ResolverPerfisPublico(evento.PublicoAlvo);

            return new CalendarioEscolarEventoViewModel
            {
                IdEventoCalendarioEscolar = evento.IdEventoCalendarioEscolar,
                Data = evento.Data,
                Tipo = evento.Tipo,
                Titulo = evento.Titulo,
                Descricao = evento.Descricao,
                PublicoAlvo = evento.PublicoAlvo,
                PerfisDestinatarios = perfisDestinatarios
                    .Select(PerfilSistema.ObterDescricaoPorId)
                    .Where(perfil => !string.IsNullOrWhiteSpace(perfil))
                    .ToArray(),
                IdUsuarioCriador = evento.IdUsuarioCriador,
                NomeUsuarioCriador = evento.NomeUsuarioCriador,
                TotalNotificados = totalNotificados,
                CriadoEmUtc = evento.CriadoEmUtc
            };
        }

        private static void ValidarPeriodo(int anoCalendario, int mesAtual)
        {
            if (anoCalendario < 1900 || anoCalendario > 2100)
            {
                throw new InvalidOperationException("Informe um ano entre 1900 e 2100.");
            }

            if (mesAtual < 1 || mesAtual > 12)
            {
                throw new InvalidOperationException("Informe um mes entre 1 e 12.");
            }
        }

        private static void ValidarEvento(CalendarioEscolarEventoCreateViewModel viewModel)
        {
            ValidarPeriodo(viewModel.Data.Year, viewModel.Data.Month);

            if (string.IsNullOrWhiteSpace(viewModel.Titulo))
            {
                throw new InvalidOperationException("Informe o titulo do evento.");
            }

            if (viewModel.Titulo.Trim().Length > 120)
            {
                throw new InvalidOperationException("O titulo do evento deve ter ate 120 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(viewModel.Descricao) && viewModel.Descricao.Trim().Length > 500)
            {
                throw new InvalidOperationException("A descricao do evento deve ter ate 500 caracteres.");
            }
        }

        private static string ResolverPublicoAlvo(string? publicoAlvo, string tipo)
        {
            if (!string.IsNullOrWhiteSpace(publicoAlvo))
            {
                return NormalizarPublicoAlvo(publicoAlvo);
            }

            return NormalizarChave(tipo) switch
            {
                "reuniaoprofessores" or "reuniaocomprofessores" => PublicoProfessores,
                "reuniaopaismestres" or "reuniaopaisemestres" or "reuniaodepaisemestres" => PublicoAlunosEProfessores,
                _ => PublicoTodos
            };
        }

        private static string NormalizarPublicoAlvo(string publicoAlvo)
        {
            return NormalizarChave(publicoAlvo) switch
            {
                "todos" or "todososperfis" => PublicoTodos,
                "professores" or "todososprofessores" => PublicoProfessores,
                "alunos" or "todososalunos" => PublicoAlunos,
                "administradores" or "administrador" or "diretoria" => PublicoAdministradores,
                "alunoseprofessores" or "professoresealunos" => PublicoAlunosEProfessores,
                _ => throw new InvalidOperationException("Publico alvo invalido para o evento do calendario.")
            };
        }

        private static int[] ResolverPerfisPublico(string publicoAlvo)
        {
            return NormalizarChave(publicoAlvo) switch
            {
                "todos" or "todososperfis" => [PerfilSistema.AdministradorId, PerfilSistema.ProfessorId, PerfilSistema.AlunoId],
                "professores" or "todososprofessores" => [PerfilSistema.ProfessorId],
                "alunos" or "todososalunos" => [PerfilSistema.AlunoId],
                "administradores" or "administrador" or "diretoria" => [PerfilSistema.AdministradorId],
                "alunoseprofessores" or "professoresealunos" => [PerfilSistema.AlunoId, PerfilSistema.ProfessorId],
                _ => [PerfilSistema.AdministradorId, PerfilSistema.ProfessorId, PerfilSistema.AlunoId]
            };
        }

        private static string NormalizarTexto(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string? NormalizarDescricao(string? descricao)
        {
            return string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        }

        private static string NormalizarChave(string value)
        {
            var normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark
                    && char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
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

        private static int GetUsuarioAtualId(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var idUsuario) ? idUsuario : 0;
        }
    }
}

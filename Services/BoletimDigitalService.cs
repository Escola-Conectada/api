using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public class BoletimDigitalService : IBoletimDigitalService
    {
        private static readonly TimeSpan CompartilhamentoValidade = TimeSpan.FromDays(7);
        private readonly DataContext _context;
        private readonly IConfiguracaoAplicacaoService? _configuracaoAplicacaoService;
        private readonly string _shareSecret;

        public BoletimDigitalService(
            DataContext context,
            IConfiguration configuration,
            IConfiguracaoAplicacaoService? configuracaoAplicacaoService = null)
        {
            _context = context;
            _configuracaoAplicacaoService = configuracaoAplicacaoService;
            _shareSecret = configuration["Boletins:ShareSecret"]
                ?? configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key nao configurada para compartilhamento de boletins.");
        }

        public async Task<BoletimDigitalViewModel> GetMeuBoletimAsync(ClaimsPrincipal principal)
        {
            var alunoUsuarioId = ValidarAluno(principal);
            var boletim = await BuildBoletimAsync(alunoUsuarioId, principal);
            return boletim ?? throw new InvalidOperationException("Aluno nao encontrado.");
        }

        public async Task<BoletimDigitalViewModel?> GetBoletimAlunoAsync(int alunoUsuarioId, ClaimsPrincipal principal)
        {
            if (!IsAdministrador(principal) && !IsProfessor(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar boletins de alunos.");
            }

            var boletim = await BuildBoletimAsync(alunoUsuarioId, principal);
            if (boletim == null)
            {
                return null;
            }

            if (IsProfessor(principal)
                && !await ProfessorPodeConsultarAlunoAsync(GetUsuarioAtualId(principal), alunoUsuarioId, boletim.IdTurmaEnsino))
            {
                throw new UnauthorizedAccessException("Professor nao autorizado a consultar este boletim.");
            }

            return boletim;
        }

        public async Task<BoletimDigitalResumoAlunoViewModel[]> GetPendentesLiberacaoAsync(ClaimsPrincipal principal)
        {
            ValidarAdministrador(principal);

            var alunosIds = await _context.BoletinsDigitais
                .AsNoTracking()
                .Where(boletim => boletim.Status == BoletimDigitalStatus.PendenteDiretoria)
                .OrderBy(boletim => boletim.SolicitadoEmUtc)
                .Select(boletim => boletim.IdAlunoUsuario)
                .Distinct()
                .ToArrayAsync();

            var pendencias = new List<BoletimDigitalResumoAlunoViewModel>();
            foreach (var alunoUsuarioId in alunosIds)
            {
                var boletim = await BuildBoletimAsync(alunoUsuarioId, principal);
                if (boletim is { Completo: true, Liberado: false, PendenteDiretoria: true })
                {
                    pendencias.Add(ToResumo(boletim));
                }
            }

            return pendencias
                .OrderBy(item => item.NomeAluno)
                .ToArray();
        }

        public async Task<BoletimDigitalViewModel?> SolicitarLiberacaoAsync(int alunoUsuarioId, ClaimsPrincipal principal)
        {
            var professorUsuarioId = ValidarProfessor(principal);
            var boletim = await BuildBoletimAsync(alunoUsuarioId, principal);
            if (boletim == null)
            {
                return null;
            }

            if (!await ProfessorPodeConsultarAlunoAsync(professorUsuarioId, alunoUsuarioId, boletim.IdTurmaEnsino))
            {
                throw new UnauthorizedAccessException("Professor nao autorizado a solicitar este boletim.");
            }

            if (!boletim.Completo)
            {
                throw new InvalidOperationException("Boletim ainda nao esta completo para envio a Diretoria.");
            }

            if (boletim.Liberado)
            {
                throw new InvalidOperationException("Boletim ja foi liberado pela Diretoria.");
            }

            var registro = await ObterOuCriarRegistroAsync(alunoUsuarioId, boletim.IdTurmaEnsino);
            if (registro.Status != BoletimDigitalStatus.PendenteDiretoria)
            {
                registro.Status = BoletimDigitalStatus.PendenteDiretoria;
                registro.IdProfessorSolicitanteUsuario = professorUsuarioId;
                registro.SolicitadoEmUtc = DateTime.UtcNow;
                registro.AtualizadoEmUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await CriarNotificacoesPendenciaDiretoriaAsync(alunoUsuarioId, professorUsuarioId, boletim);
            }

            return await BuildBoletimAsync(alunoUsuarioId, principal);
        }

        public async Task<BoletimDigitalViewModel?> LiberarAsync(int alunoUsuarioId, ClaimsPrincipal principal)
        {
            var administradorUsuarioId = ValidarAdministrador(principal);
            var boletim = await BuildBoletimAsync(alunoUsuarioId, principal);
            if (boletim == null)
            {
                return null;
            }

            if (!boletim.Completo)
            {
                throw new InvalidOperationException("Boletim ainda nao esta completo para liberacao.");
            }

            if (!boletim.PendenteDiretoria && !boletim.Liberado)
            {
                throw new InvalidOperationException("Professor ainda nao enviou este boletim para a Diretoria.");
            }

            var registro = await ObterOuCriarRegistroAsync(alunoUsuarioId, boletim.IdTurmaEnsino);
            var estavaLiberado = registro.Status == BoletimDigitalStatus.Liberado;
            registro.Status = BoletimDigitalStatus.Liberado;
            registro.IdAdministradorLiberacaoUsuario = administradorUsuarioId;
            registro.LiberadoEmUtc ??= DateTime.UtcNow;
            registro.AtualizadoEmUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (!estavaLiberado)
            {
                await CriarNotificacaoBoletimLiberadoAsync(alunoUsuarioId, administradorUsuarioId, boletim);
            }

            return await BuildBoletimAsync(alunoUsuarioId, principal);
        }

        public async Task<ArquivoDownload?> DownloadMeuBoletimPdfAsync(ClaimsPrincipal principal)
        {
            var alunoUsuarioId = ValidarAluno(principal);
            return await CriarPdfAsync(alunoUsuarioId, principal);
        }

        public async Task<ArquivoDownload?> DownloadBoletimAlunoPdfAsync(int alunoUsuarioId, ClaimsPrincipal principal)
        {
            ValidarAdministrador(principal);
            return await CriarPdfAsync(alunoUsuarioId, principal);
        }

        public async Task<ArquivoDownload?> DownloadBoletimCompartilhadoAsync(string token)
        {
            var dados = ValidarTokenCompartilhamento(token);
            if (dados == null)
            {
                return null;
            }

            var boletim = await BuildBoletimAsync(dados.Value.AlunoUsuarioId);
            if (boletim == null
                || boletim.IdTurmaEnsino != dados.Value.TurmaEnsinoId
                || !boletim.Completo
                || !boletim.Liberado)
            {
                return null;
            }

            return BoletimDigitalPdfBuilder.Criar(boletim, await GetNomeEscolaAsync());
        }

        public async Task<BoletimDigitalCompartilhamentoViewModel?> CriarCompartilhamentoMeuBoletimAsync(ClaimsPrincipal principal)
        {
            var alunoUsuarioId = ValidarAluno(principal);
            return await CriarCompartilhamentoAsync(alunoUsuarioId, principal);
        }

        public async Task<BoletimDigitalCompartilhamentoViewModel?> CriarCompartilhamentoBoletimAlunoAsync(
            int alunoUsuarioId,
            ClaimsPrincipal principal)
        {
            ValidarAdministrador(principal);
            return await CriarCompartilhamentoAsync(alunoUsuarioId, principal);
        }

        private async Task<ArquivoDownload?> CriarPdfAsync(int alunoUsuarioId, ClaimsPrincipal principal)
        {
            var boletim = await BuildBoletimAsync(alunoUsuarioId, principal);
            if (boletim == null)
            {
                return null;
            }

            if (!boletim.Completo || !boletim.Liberado)
            {
                throw new InvalidOperationException("Boletim precisa estar completo e liberado pela Diretoria.");
            }

            return BoletimDigitalPdfBuilder.Criar(boletim, await GetNomeEscolaAsync());
        }

        private async Task<BoletimDigitalCompartilhamentoViewModel?> CriarCompartilhamentoAsync(
            int alunoUsuarioId,
            ClaimsPrincipal principal)
        {
            var boletim = await BuildBoletimAsync(alunoUsuarioId, principal);
            if (boletim == null)
            {
                return null;
            }

            if (!boletim.Completo || !boletim.Liberado)
            {
                throw new InvalidOperationException("Boletim precisa estar completo e liberado pela Diretoria.");
            }

            var expiraEmUtc = DateTime.UtcNow.Add(CompartilhamentoValidade);
            return new BoletimDigitalCompartilhamentoViewModel
            {
                Token = CriarTokenCompartilhamento(alunoUsuarioId, boletim.IdTurmaEnsino, expiraEmUtc),
                ExpiraEmUtc = expiraEmUtc,
                Texto = $"Boletim escolar digital de {boletim.NomeAluno} - {boletim.NomeTurmaEnsino}. Documento completo e liberado pela Diretoria."
            };
        }

        private async Task<BoletimDigitalViewModel?> BuildBoletimAsync(
            int alunoUsuarioId,
            ClaimsPrincipal? principal = null)
        {
            var aluno = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(usuario => usuario.IdUsuario == alunoUsuarioId && usuario.IdPerfil == PerfilSistema.AlunoId);

            if (aluno == null)
            {
                return null;
            }

            var matricula = await _context.AlunosTurmasEnsino
                .Include(item => item.TurmaEnsino)
                    .ThenInclude(turma => turma!.TipoEnsino)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.IdAlunoUsuario == alunoUsuarioId);

            if (matricula?.TurmaEnsino == null)
            {
                throw new InvalidOperationException("Aluno nao matriculado em nenhuma turma.");
            }

            var disciplinas = await _context.Disciplinas
                .Include(disciplina => disciplina.ProfessorUsuario)
                .Include(disciplina => disciplina.AreaConhecimento)
                .AsNoTracking()
                .Where(disciplina => disciplina.IdTurmaEnsino == matricula.IdTurmaEnsino)
                .OrderBy(disciplina => disciplina.AreaConhecimento == null ? 0 : disciplina.AreaConhecimento.Ordem)
                .ThenBy(disciplina => disciplina.Ordem)
                .ThenBy(disciplina => disciplina.Nome)
                .ToArrayAsync();

            var disciplinasIds = disciplinas.Select(disciplina => disciplina.IdDisciplina).ToArray();
            var cadernetas = await _context.CadernetasDigitais
                .Include(caderneta => caderneta.ProfessorUsuario)
                .AsNoTracking()
                .Where(caderneta =>
                    caderneta.IdAlunoUsuario == alunoUsuarioId
                    && caderneta.IdTurmaEnsino == matricula.IdTurmaEnsino
                    && disciplinasIds.Contains(caderneta.IdDisciplina))
                .ToArrayAsync();

            var boletim = await _context.BoletinsDigitais
                .Include(item => item.ProfessorSolicitanteUsuario)
                .Include(item => item.AdministradorLiberacaoUsuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.IdAlunoUsuario == alunoUsuarioId
                    && item.IdTurmaEnsino == matricula.IdTurmaEnsino);

            var disciplinasBoletim = disciplinas
                .Select(disciplina => ToDisciplinaBoletim(disciplina, cadernetas.FirstOrDefault(item => item.IdDisciplina == disciplina.IdDisciplina)))
                .ToArray();

            var lancadas = disciplinasBoletim.Where(item => item.Lancado).ToArray();
            var completo = disciplinasBoletim.Length > 0 && lancadas.Length == disciplinasBoletim.Length;
            var status = boletim?.Status ?? BoletimDigitalStatus.EmAberto;
            var liberado = status == BoletimDigitalStatus.Liberado;
            var pendenteDiretoria = status == BoletimDigitalStatus.PendenteDiretoria;
            var mediaGeral = lancadas.Length == 0
                ? (decimal?)null
                : Math.Round(lancadas.Average(item => item.MediaAritmetica ?? 0), 2, MidpointRounding.AwayFromZero);

            var viewModel = new BoletimDigitalViewModel
            {
                IdBoletimDigital = boletim?.IdBoletimDigital,
                IdAlunoUsuario = aluno.IdUsuario,
                NomeAluno = aluno.Nome,
                EmailAluno = aluno.Email,
                IdTipoEnsino = matricula.TurmaEnsino.IdTipoEnsino,
                NomeTipoEnsino = matricula.TurmaEnsino.TipoEnsino?.Nome ?? string.Empty,
                IdTurmaEnsino = matricula.IdTurmaEnsino,
                NomeTurmaEnsino = matricula.TurmaEnsino.Nome,
                Status = status,
                Completo = completo,
                Liberado = liberado,
                PendenteDiretoria = pendenteDiretoria,
                PodeCompartilhar = completo && liberado,
                TotalDisciplinas = disciplinasBoletim.Length,
                DisciplinasLancadas = lancadas.Length,
                DisciplinasPendentes = disciplinasBoletim.Length - lancadas.Length,
                MediaGeral = mediaGeral,
                SituacaoGeral = CalcularSituacaoGeral(completo, disciplinasBoletim),
                TotalPresencas = lancadas.Sum(item => item.Presencas ?? 0),
                TotalFaltas = lancadas.Sum(item => item.Faltas ?? 0),
                IdProfessorSolicitanteUsuario = boletim?.IdProfessorSolicitanteUsuario,
                NomeProfessorSolicitante = boletim?.ProfessorSolicitanteUsuario?.Nome ?? string.Empty,
                SolicitadoEmUtc = boletim?.SolicitadoEmUtc,
                IdAdministradorLiberacaoUsuario = boletim?.IdAdministradorLiberacaoUsuario,
                NomeAdministradorLiberacao = boletim?.AdministradorLiberacaoUsuario?.Nome ?? string.Empty,
                LiberadoEmUtc = boletim?.LiberadoEmUtc,
                Disciplinas = disciplinasBoletim
            };

            if (principal != null)
            {
                viewModel.PodeSolicitarLiberacao = IsProfessor(principal)
                    && completo
                    && !pendenteDiretoria
                    && !liberado
                    && await ProfessorPodeConsultarAlunoAsync(GetUsuarioAtualId(principal), alunoUsuarioId, matricula.IdTurmaEnsino);
                viewModel.PodeLiberar = IsAdministrador(principal)
                    && completo
                    && pendenteDiretoria
                    && !liberado;
            }

            return viewModel;
        }

        private static BoletimDigitalDisciplinaViewModel ToDisciplinaBoletim(
            Disciplina disciplina,
            CadernetaDigital? caderneta)
        {
            if (caderneta == null)
            {
                return new BoletimDigitalDisciplinaViewModel
                {
                    IdDisciplina = disciplina.IdDisciplina,
                    NomeDisciplina = disciplina.Nome,
                    IdProfessorUsuario = disciplina.IdProfessorUsuario,
                    NomeProfessor = disciplina.ProfessorUsuario?.Nome ?? string.Empty,
                    IdAreaConhecimento = disciplina.IdAreaConhecimento,
                    NomeAreaConhecimento = disciplina.AreaConhecimento?.Nome,
                    OfertaObrigatoria = disciplina.OfertaObrigatoria,
                    MatriculaFacultativa = disciplina.MatriculaFacultativa,
                    Ordem = disciplina.Ordem,
                    Lancado = false,
                    Situacao = "Pendente",
                    CorSituacao = "cinza"
                };
            }

            var notas = DeserializeNotas(caderneta.Notas);
            var media = CalcularMediaAritmetica(notas);
            var situacao = CalcularSituacao(media, caderneta.Faltas);

            return new BoletimDigitalDisciplinaViewModel
            {
                IdDisciplina = disciplina.IdDisciplina,
                NomeDisciplina = disciplina.Nome,
                IdProfessorUsuario = caderneta.IdProfessorUsuario ?? disciplina.IdProfessorUsuario,
                NomeProfessor = caderneta.ProfessorUsuario?.Nome ?? disciplina.ProfessorUsuario?.Nome ?? string.Empty,
                IdAreaConhecimento = disciplina.IdAreaConhecimento,
                NomeAreaConhecimento = disciplina.AreaConhecimento?.Nome,
                OfertaObrigatoria = disciplina.OfertaObrigatoria,
                MatriculaFacultativa = disciplina.MatriculaFacultativa,
                Ordem = disciplina.Ordem,
                Lancado = true,
                IdCadernetaDigital = caderneta.IdCadernetaDigital,
                Notas = notas,
                MediaAritmetica = media,
                Situacao = situacao.Label,
                CorSituacao = situacao.Cor,
                Presencas = caderneta.Presencas,
                Faltas = caderneta.Faltas
            };
        }

        private async Task<BoletimDigital> ObterOuCriarRegistroAsync(int alunoUsuarioId, int turmaEnsinoId)
        {
            var registro = await _context.BoletinsDigitais
                .FirstOrDefaultAsync(item =>
                    item.IdAlunoUsuario == alunoUsuarioId
                    && item.IdTurmaEnsino == turmaEnsinoId);

            if (registro != null)
            {
                return registro;
            }

            registro = new BoletimDigital
            {
                IdAlunoUsuario = alunoUsuarioId,
                IdTurmaEnsino = turmaEnsinoId,
                Status = BoletimDigitalStatus.EmAberto,
                AtualizadoEmUtc = DateTime.UtcNow
            };

            _context.BoletinsDigitais.Add(registro);
            await _context.SaveChangesAsync();
            return registro;
        }

        private async Task<bool> ProfessorPodeConsultarAlunoAsync(int professorUsuarioId, int alunoUsuarioId, int turmaEnsinoId)
        {
            if (professorUsuarioId <= 0)
            {
                return false;
            }

            return await _context.CadernetasDigitais
                .AsNoTracking()
                .AnyAsync(caderneta =>
                    caderneta.IdAlunoUsuario == alunoUsuarioId
                    && caderneta.IdTurmaEnsino == turmaEnsinoId
                    && caderneta.IdProfessorUsuario == professorUsuarioId)
                || await _context.Disciplinas
                    .AsNoTracking()
                    .AnyAsync(disciplina =>
                        disciplina.IdTurmaEnsino == turmaEnsinoId
                        && disciplina.IdProfessorUsuario == professorUsuarioId);
        }

        private async Task CriarNotificacoesPendenciaDiretoriaAsync(
            int alunoUsuarioId,
            int professorUsuarioId,
            BoletimDigitalViewModel boletim)
        {
            var professor = await _context.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.IdUsuario == professorUsuarioId)
                .Select(usuario => usuario.Nome)
                .FirstOrDefaultAsync() ?? "Professor";

            var administradoresIds = await _context.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.IdPerfil == PerfilSistema.AdministradorId)
                .Select(usuario => usuario.IdUsuario)
                .ToArrayAsync();

            var criadaEmUtc = DateTime.UtcNow;
            var notificacoes = administradoresIds.Select(idUsuario => new Notificacao
            {
                IdUsuario = idUsuario,
                Tipo = "BoletimPendenteLiberacao",
                Titulo = $"Boletim pronto para liberacao - {boletim.NomeAluno}",
                Mensagem = $"O professor {professor} concluiu os lancamentos de {boletim.NomeAluno} na turma {boletim.NomeTurmaEnsino}. O boletim digital esta pronto para liberacao pela Diretoria.",
                Link = $"/boletim-digital/admin?alunoUsuarioId={alunoUsuarioId}",
                CriadaEmUtc = criadaEmUtc
            });

            _context.Notificacoes.AddRange(notificacoes);
            await _context.SaveChangesAsync();
        }

        private async Task CriarNotificacaoBoletimLiberadoAsync(
            int alunoUsuarioId,
            int administradorUsuarioId,
            BoletimDigitalViewModel boletim)
        {
            var administrador = await _context.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.IdUsuario == administradorUsuarioId)
                .Select(usuario => usuario.Nome)
                .FirstOrDefaultAsync() ?? "Diretoria";

            _context.Notificacoes.Add(new Notificacao
            {
                IdUsuario = alunoUsuarioId,
                Tipo = "BoletimLiberado",
                Titulo = "Boletim digital liberado",
                Mensagem = $"A Diretoria liberou seu boletim digital da turma {boletim.NomeTurmaEnsino}. Liberado por {administrador}.",
                Link = "/boletim-digital",
                CriadaEmUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        private async Task<string> GetNomeEscolaAsync()
        {
            return _configuracaoAplicacaoService == null
                ? ConfiguracaoAplicacaoService.NomeEscolaPadrao
                : await _configuracaoAplicacaoService.GetNomeEscolaAsync();
        }

        private static BoletimDigitalResumoAlunoViewModel ToResumo(BoletimDigitalViewModel boletim)
        {
            return new BoletimDigitalResumoAlunoViewModel
            {
                IdBoletimDigital = boletim.IdBoletimDigital,
                IdAlunoUsuario = boletim.IdAlunoUsuario,
                NomeAluno = boletim.NomeAluno,
                EmailAluno = boletim.EmailAluno,
                IdTurmaEnsino = boletim.IdTurmaEnsino,
                NomeTurmaEnsino = boletim.NomeTurmaEnsino,
                Status = boletim.Status,
                Completo = boletim.Completo,
                Liberado = boletim.Liberado,
                PendenteLiberacao = boletim.PendenteDiretoria,
                TotalDisciplinas = boletim.TotalDisciplinas,
                DisciplinasLancadas = boletim.DisciplinasLancadas,
                DisciplinasPendentes = boletim.DisciplinasPendentes,
                SolicitadoEmUtc = boletim.SolicitadoEmUtc,
                NomeProfessorSolicitante = boletim.NomeProfessorSolicitante
            };
        }

        private string CriarTokenCompartilhamento(int alunoUsuarioId, int turmaEnsinoId, DateTime expiraEmUtc)
        {
            var expiraUnix = new DateTimeOffset(expiraEmUtc).ToUnixTimeSeconds();
            var payload = $"{alunoUsuarioId}:{turmaEnsinoId}:{expiraUnix}";
            var assinatura = Assinar(payload);

            return $"{Base64UrlEncode(Encoding.UTF8.GetBytes(payload))}.{Base64UrlEncode(assinatura)}";
        }

        private (int AlunoUsuarioId, int TurmaEnsinoId)? ValidarTokenCompartilhamento(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var partes = token.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length != 2)
            {
                return null;
            }

            string payload;
            byte[] assinaturaInformada;
            try
            {
                payload = Encoding.UTF8.GetString(Base64UrlDecode(partes[0]));
                assinaturaInformada = Base64UrlDecode(partes[1]);
            }
            catch
            {
                return null;
            }

            var dados = payload.Split(':');
            if (dados.Length != 3
                || !int.TryParse(dados[0], out var alunoUsuarioId)
                || !int.TryParse(dados[1], out var turmaEnsinoId)
                || !long.TryParse(dados[2], out var expiraUnix))
            {
                return null;
            }

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiraUnix)
            {
                return null;
            }

            var assinaturaEsperada = Assinar(payload);
            if (!CryptographicOperations.FixedTimeEquals(assinaturaInformada, assinaturaEsperada))
            {
                return null;
            }

            return (alunoUsuarioId, turmaEnsinoId);
        }

        private byte[] Assinar(string payload)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_shareSecret));
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        }

        private static string Base64UrlEncode(byte[] value)
        {
            return Convert.ToBase64String(value)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string value)
        {
            var base64 = value
                .Replace('-', '+')
                .Replace('_', '/');

            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }

            return Convert.FromBase64String(base64);
        }

        private static decimal[] DeserializeNotas(string value)
        {
            return value
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(item => decimal.TryParse(item, NumberStyles.Number, CultureInfo.InvariantCulture, out var nota) ? nota : (decimal?)null)
                .Where(nota => nota.HasValue)
                .Select(nota => nota!.Value)
                .ToArray();
        }

        private static decimal CalcularMediaAritmetica(decimal[] notas)
        {
            return notas.Length == 0
                ? 0
                : Math.Round(notas.Average(), 2, MidpointRounding.AwayFromZero);
        }

        private static (string Label, string Cor) CalcularSituacao(decimal media, int faltas)
        {
            if (faltas >= 10)
            {
                return ("Reprovado por Faltas", "vermelho");
            }

            if (media < 6)
            {
                return ("Reprovado", "vermelho");
            }

            if (media <= 7)
            {
                return ("Em recuperacao", "preto");
            }

            return ("Aprovado", "azul");
        }

        private static string CalcularSituacaoGeral(bool completo, BoletimDigitalDisciplinaViewModel[] disciplinas)
        {
            if (!completo)
            {
                return "Em lancamento";
            }

            if (disciplinas.Any(item => item.Situacao == "Reprovado por Faltas"))
            {
                return "Reprovado por Faltas";
            }

            if (disciplinas.Any(item => item.Situacao == "Reprovado"))
            {
                return "Reprovado";
            }

            if (disciplinas.Any(item => item.Situacao == "Em recuperacao"))
            {
                return "Em recuperacao";
            }

            return "Aprovado";
        }

        private static int ValidarAluno(ClaimsPrincipal principal)
        {
            if (!principal.IsInRole(PerfilSistema.Aluno))
            {
                throw new UnauthorizedAccessException("Apenas alunos podem consultar o proprio boletim.");
            }

            var usuarioId = GetUsuarioAtualId(principal);
            if (usuarioId <= 0)
            {
                throw new UnauthorizedAccessException("Sessao invalida para consultar boletim.");
            }

            return usuarioId;
        }

        private static int ValidarProfessor(ClaimsPrincipal principal)
        {
            if (!principal.IsInRole(PerfilSistema.Professor))
            {
                throw new UnauthorizedAccessException("Apenas professores podem solicitar liberacao de boletim.");
            }

            var usuarioId = GetUsuarioAtualId(principal);
            if (usuarioId <= 0)
            {
                throw new UnauthorizedAccessException("Sessao invalida para solicitar boletim.");
            }

            return usuarioId;
        }

        private static int ValidarAdministrador(ClaimsPrincipal principal)
        {
            if (!principal.IsInRole(PerfilSistema.Administrador))
            {
                throw new UnauthorizedAccessException("Apenas administradores podem liberar boletins.");
            }

            var usuarioId = GetUsuarioAtualId(principal);
            if (usuarioId <= 0)
            {
                throw new UnauthorizedAccessException("Sessao invalida para liberar boletim.");
            }

            return usuarioId;
        }

        private static bool IsAdministrador(ClaimsPrincipal principal)
        {
            return principal.IsInRole(PerfilSistema.Administrador);
        }

        private static bool IsProfessor(ClaimsPrincipal principal)
        {
            return principal.IsInRole(PerfilSistema.Professor);
        }

        private static int GetUsuarioAtualId(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var idUsuario) ? idUsuario : 0;
        }
    }
}

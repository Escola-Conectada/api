using System.Globalization;
using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Security;
using ESCOLA_API.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public class CadernetaDigitalService : ICadernetaDigitalService
    {
        private readonly DataContext _context;
        private readonly ICadernetaDigitalEventPublisher _eventPublisher;

        public CadernetaDigitalService(DataContext context)
            : this(context, new DatabaseCadernetaDigitalEventPublisher(context))
        {
        }

        public CadernetaDigitalService(DataContext context, ICadernetaDigitalEventPublisher eventPublisher)
        {
            _context = context;
            _eventPublisher = eventPublisher;
        }

        public async Task<CadernetaDigitalViewModel[]> GetAllAsync(ClaimsPrincipal principal)
        {
            var query = CadernetaQuery();

            if (IsAluno(principal))
            {
                throw new UnauthorizedAccessException("Aluno deve consultar o boletim digital.");
            }
            else if (IsProfessor(principal))
            {
                var usuarioId = GetUsuarioAtualId(principal);
                query = query.Where(caderneta =>
                    caderneta.IdProfessorUsuario == usuarioId
                    || caderneta.Disciplina!.IdProfessorUsuario == usuarioId);
            }
            else if (!IsProfessor(principal) && !IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar a caderneta.");
            }

            var cadernetas = await query
                .OrderBy(caderneta => caderneta.AlunoUsuario!.Nome)
                .ThenBy(caderneta => caderneta.IdTipoEnsino ?? caderneta.Disciplina!.TurmaEnsino!.IdTipoEnsino)
                .ThenBy(caderneta => caderneta.IdTurmaEnsino ?? caderneta.Disciplina!.IdTurmaEnsino)
                .ThenBy(caderneta => caderneta.Disciplina!.AreaConhecimento!.Ordem)
                .ThenBy(caderneta => caderneta.Disciplina!.Nome)
                .ToArrayAsync();

            return cadernetas.Select(ToViewModel).ToArray();
        }

        public async Task<CadernetaDigitalViewModel?> GetByIdAsync(int cadernetaId, ClaimsPrincipal principal)
        {
            var caderneta = await CadernetaQuery()
                .FirstOrDefaultAsync(item => item.IdCadernetaDigital == cadernetaId);

            if (caderneta != null && !PodeConsultar(principal, caderneta))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar este lancamento.");
            }

            return caderneta == null ? null : ToViewModel(caderneta);
        }

        public async Task<CadernetaDigitalViewModel> AddAsync(CadernetaDigitalCreateUpdateViewModel viewModel, ClaimsPrincipal principal)
        {
            var usuarioId = await ValidarProfessorAsync(principal);
            var disciplina = await ObterDisciplinaParaLancamentoAsync(
                viewModel.IdDisciplina,
                viewModel.IdTipoEnsino,
                viewModel.IdTurmaEnsino,
                usuarioId);
            var aluno = await ObterAlunoAsync(viewModel.IdAlunoUsuario);

            if (disciplina == null)
            {
                throw new InvalidOperationException("Disciplina nao encontrada para o tipo de ensino e turma informados.");
            }

            if (aluno == null)
            {
                throw new InvalidOperationException("Aluno nao encontrado.");
            }

            await ValidarMatriculaAlunoTurmaAsync(aluno.IdUsuario, viewModel.IdTurmaEnsino);

            var jaAssociado = await _context.CadernetasDigitais
                .AnyAsync(caderneta => caderneta.IdAlunoUsuario == aluno.IdUsuario && caderneta.IdDisciplina == disciplina.IdDisciplina);

            if (jaAssociado)
            {
                throw new InvalidOperationException("Este aluno ja esta associado a esta disciplina.");
            }

            var caderneta = new CadernetaDigital
            {
                IdAlunoUsuario = aluno.IdUsuario,
                IdProfessorUsuario = usuarioId,
                IdTipoEnsino = viewModel.IdTipoEnsino,
                IdTurmaEnsino = viewModel.IdTurmaEnsino,
                IdDisciplina = disciplina.IdDisciplina,
                Notas = SerializeNotas(viewModel.Notas),
                Presencas = viewModel.Presencas,
                Faltas = viewModel.Faltas
            };

            _context.CadernetasDigitais.Add(caderneta);
            await SaveChangesAsync("Este aluno ja esta associado a esta disciplina.");

            var created = (await GetByIdAsync(caderneta.IdCadernetaDigital, principal))!;
            await CriarNotificacaoLancamentoAsync(created, "Criacao");
            return created;
        }

        public async Task<CadernetaDigitalViewModel?> UpdateAsync(int cadernetaId, CadernetaDigitalCreateUpdateViewModel viewModel, ClaimsPrincipal principal)
        {
            var usuarioId = await ValidarProfessorAsync(principal);
            var caderneta = await _context.CadernetasDigitais
                .Include(item => item.Disciplina)
                .FirstOrDefaultAsync(item => item.IdCadernetaDigital == cadernetaId);

            if (caderneta == null)
            {
                return null;
            }

            if (!PodeAdministrarLancamento(caderneta, usuarioId))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a alterar este lancamento.");
            }

            var disciplina = await ObterDisciplinaParaLancamentoAsync(
                viewModel.IdDisciplina,
                viewModel.IdTipoEnsino,
                viewModel.IdTurmaEnsino,
                usuarioId);
            var aluno = await ObterAlunoAsync(viewModel.IdAlunoUsuario);

            if (disciplina == null)
            {
                throw new InvalidOperationException("Disciplina nao encontrada para o tipo de ensino e turma informados.");
            }

            if (aluno == null)
            {
                throw new InvalidOperationException("Aluno nao encontrado.");
            }

            await ValidarMatriculaAlunoTurmaAsync(aluno.IdUsuario, viewModel.IdTurmaEnsino);

            var jaAssociado = await _context.CadernetasDigitais
                .AnyAsync(item =>
                    item.IdCadernetaDigital != cadernetaId
                    && item.IdAlunoUsuario == aluno.IdUsuario
                    && item.IdDisciplina == disciplina.IdDisciplina);

            if (jaAssociado)
            {
                throw new InvalidOperationException("Este aluno ja esta associado a esta disciplina.");
            }

            caderneta.IdAlunoUsuario = aluno.IdUsuario;
            caderneta.IdProfessorUsuario = usuarioId;
            caderneta.IdTipoEnsino = viewModel.IdTipoEnsino;
            caderneta.IdTurmaEnsino = viewModel.IdTurmaEnsino;
            caderneta.IdDisciplina = disciplina.IdDisciplina;
            caderneta.Notas = SerializeNotas(viewModel.Notas);
            caderneta.Presencas = viewModel.Presencas;
            caderneta.Faltas = viewModel.Faltas;

            await SaveChangesAsync("Este aluno ja esta associado a esta disciplina.");
            var updated = await GetByIdAsync(cadernetaId, principal);
            if (updated != null)
            {
                await CriarNotificacaoLancamentoAsync(updated, "Atualizacao");
            }

            return updated;
        }

        public async Task<bool> DeleteAsync(int cadernetaId, ClaimsPrincipal principal)
        {
            var usuarioId = await ValidarProfessorAsync(principal);
            var caderneta = await _context.CadernetasDigitais
                .Include(item => item.Disciplina)
                .FirstOrDefaultAsync(item => item.IdCadernetaDigital == cadernetaId);

            if (caderneta == null)
            {
                return false;
            }

            if (!PodeAdministrarLancamento(caderneta, usuarioId))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a excluir este lancamento.");
            }

            _context.CadernetasDigitais.Remove(caderneta);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DisciplinaViewModel[]> GetDisciplinasAsync(ClaimsPrincipal principal)
        {
            var query = _context.Disciplinas
                .Include(disciplina => disciplina.ProfessorUsuario)
                .Include(disciplina => disciplina.TurmaEnsino)
                    .ThenInclude(turma => turma!.TipoEnsino)
                .Include(disciplina => disciplina.AreaConhecimento)
                .AsNoTracking();

            if (IsProfessor(principal))
            {
                var usuarioId = GetUsuarioAtualId(principal);
                query = query.Where(disciplina => disciplina.IdProfessorUsuario == usuarioId);
            }
            else if (IsAluno(principal))
            {
                throw new UnauthorizedAccessException("Aluno deve consultar as disciplinas pelo boletim digital.");
            }
            else if (!IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar disciplinas.");
            }

            var disciplinas = await query
                .OrderBy(disciplina => disciplina.TurmaEnsino!.TipoEnsino!.Ordem)
                .ThenBy(disciplina => disciplina.TurmaEnsino!.Ordem)
                .ThenBy(disciplina => disciplina.AreaConhecimento!.Ordem)
                .ThenBy(disciplina => disciplina.Nome)
                .ToArrayAsync();

            return disciplinas.Select(ToViewModel).ToArray();
        }

        public async Task<TipoEnsinoCurricularViewModel[]> GetEstruturaEnsinoAsync(ClaimsPrincipal principal)
        {
            if (IsAluno(principal))
            {
                throw new UnauthorizedAccessException("Aluno deve consultar a estrutura pelo boletim digital.");
            }

            if (!IsAdministrador(principal) && !IsProfessor(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar a estrutura de ensino.");
            }

            var disciplinas = await _context.Disciplinas
                .Include(disciplina => disciplina.TurmaEnsino)
                    .ThenInclude(turma => turma!.TipoEnsino)
                .Include(disciplina => disciplina.AreaConhecimento)
                .AsNoTracking()
                .Where(disciplina =>
                    disciplina.IdProfessorUsuario == null
                    && disciplina.IdTurmaEnsino != null
                    && disciplina.IdAreaConhecimento != null)
                .OrderBy(disciplina => disciplina.TurmaEnsino!.TipoEnsino!.Ordem)
                .ThenBy(disciplina => disciplina.TurmaEnsino!.Ordem)
                .ThenBy(disciplina => disciplina.AreaConhecimento!.Ordem)
                .ThenBy(disciplina => disciplina.Ordem)
                .ThenBy(disciplina => disciplina.Nome)
                .ToArrayAsync();

            return disciplinas
                .GroupBy(disciplina => new
                {
                    disciplina.TurmaEnsino!.TipoEnsino!.IdTipoEnsino,
                    disciplina.TurmaEnsino.TipoEnsino.Nome,
                    disciplina.TurmaEnsino.TipoEnsino.Ordem
                })
                .OrderBy(group => group.Key.Ordem)
                .Select(tipoGroup => new TipoEnsinoCurricularViewModel
                {
                    IdTipoEnsino = tipoGroup.Key.IdTipoEnsino,
                    Nome = tipoGroup.Key.Nome,
                    Ordem = tipoGroup.Key.Ordem,
                    Turmas = tipoGroup
                        .GroupBy(disciplina => new
                        {
                            disciplina.TurmaEnsino!.IdTurmaEnsino,
                            disciplina.TurmaEnsino.Nome,
                            disciplina.TurmaEnsino.Codigo,
                            disciplina.TurmaEnsino.Ordem
                        })
                        .OrderBy(group => group.Key.Ordem)
                        .Select(turmaGroup => new TurmaEnsinoCurricularViewModel
                        {
                            IdTurmaEnsino = turmaGroup.Key.IdTurmaEnsino,
                            Nome = turmaGroup.Key.Nome,
                            Codigo = turmaGroup.Key.Codigo,
                            Ordem = turmaGroup.Key.Ordem,
                            AreasConhecimento = turmaGroup
                                .GroupBy(disciplina => new
                                {
                                    disciplina.AreaConhecimento!.IdAreaConhecimento,
                                    disciplina.AreaConhecimento.Nome,
                                    disciplina.AreaConhecimento.Ordem
                                })
                                .OrderBy(group => group.Key.Ordem)
                                .Select(areaGroup => new AreaConhecimentoCurricularViewModel
                                {
                                    IdAreaConhecimento = areaGroup.Key.IdAreaConhecimento,
                                    Nome = areaGroup.Key.Nome,
                                    Ordem = areaGroup.Key.Ordem,
                                    Disciplinas = areaGroup
                                        .OrderBy(disciplina => disciplina.Ordem)
                                        .ThenBy(disciplina => disciplina.Nome)
                                        .Select(disciplina => new DisciplinaCurricularViewModel
                                        {
                                            IdDisciplina = disciplina.IdDisciplina,
                                            Nome = disciplina.Nome,
                                            Observacao = disciplina.Observacao,
                                            OfertaObrigatoria = disciplina.OfertaObrigatoria,
                                            MatriculaFacultativa = disciplina.MatriculaFacultativa,
                                            Ordem = disciplina.Ordem
                                        })
                                        .ToArray()
                                })
                                .ToArray()
                        })
                        .ToArray()
                })
                .ToArray();
        }

        public async Task<DisciplinaViewModel> AddDisciplinaAsync(DisciplinaCreateUpdateViewModel viewModel, ClaimsPrincipal principal)
        {
            var usuarioId = await ValidarProfessorAsync(principal);
            var nome = viewModel.Nome.Trim();

            await ValidarEstruturaCurricularAsync(viewModel.IdTurmaEnsino, viewModel.IdAreaConhecimento);
            var jaExiste = await DisciplinaJaExisteAsync(nome, usuarioId, viewModel.IdTurmaEnsino);

            if (jaExiste)
            {
                throw new InvalidOperationException("Disciplina ja cadastrada.");
            }

            var disciplina = new Disciplina
            {
                Nome = nome,
                IdProfessorUsuario = usuarioId,
                IdTurmaEnsino = viewModel.IdTurmaEnsino,
                IdAreaConhecimento = viewModel.IdAreaConhecimento,
                Observacao = NormalizarTextoOpcional(viewModel.Observacao),
                OfertaObrigatoria = viewModel.OfertaObrigatoria,
                MatriculaFacultativa = viewModel.MatriculaFacultativa
            };

            _context.Disciplinas.Add(disciplina);
            await SaveChangesAsync("Disciplina ja cadastrada.");

            var created = await _context.Disciplinas
                .Include(item => item.ProfessorUsuario)
                .Include(item => item.TurmaEnsino)
                    .ThenInclude(turma => turma!.TipoEnsino)
                .Include(item => item.AreaConhecimento)
                .AsNoTracking()
                .FirstAsync(item => item.IdDisciplina == disciplina.IdDisciplina);

            return ToViewModel(created);
        }

        public async Task<DisciplinaViewModel?> UpdateDisciplinaAsync(int disciplinaId, DisciplinaCreateUpdateViewModel viewModel, ClaimsPrincipal principal)
        {
            var usuarioId = await ValidarProfessorAsync(principal);
            var disciplina = await _context.Disciplinas
                .FirstOrDefaultAsync(item => item.IdDisciplina == disciplinaId);

            if (disciplina == null)
            {
                return null;
            }

            if (disciplina.IdProfessorUsuario != usuarioId)
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a alterar esta disciplina.");
            }

            var nome = viewModel.Nome.Trim();
            await ValidarEstruturaCurricularAsync(viewModel.IdTurmaEnsino, viewModel.IdAreaConhecimento);
            var jaExiste = await DisciplinaJaExisteAsync(nome, usuarioId, viewModel.IdTurmaEnsino, disciplinaId);

            if (jaExiste)
            {
                throw new InvalidOperationException("Disciplina ja cadastrada.");
            }

            disciplina.Nome = nome;
            disciplina.IdTurmaEnsino = viewModel.IdTurmaEnsino;
            disciplina.IdAreaConhecimento = viewModel.IdAreaConhecimento;
            disciplina.Observacao = NormalizarTextoOpcional(viewModel.Observacao);
            disciplina.OfertaObrigatoria = viewModel.OfertaObrigatoria;
            disciplina.MatriculaFacultativa = viewModel.MatriculaFacultativa;
            await SaveChangesAsync("Disciplina ja cadastrada.");

            var updated = await _context.Disciplinas
                .Include(item => item.ProfessorUsuario)
                .Include(item => item.TurmaEnsino)
                    .ThenInclude(turma => turma!.TipoEnsino)
                .Include(item => item.AreaConhecimento)
                .AsNoTracking()
                .FirstAsync(item => item.IdDisciplina == disciplinaId);

            return ToViewModel(updated);
        }

        public async Task<bool> DeleteDisciplinaAsync(int disciplinaId, ClaimsPrincipal principal)
        {
            var usuarioId = await ValidarProfessorAsync(principal);
            var disciplina = await _context.Disciplinas
                .FirstOrDefaultAsync(item => item.IdDisciplina == disciplinaId);

            if (disciplina == null)
            {
                return false;
            }

            if (disciplina.IdProfessorUsuario != usuarioId)
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a excluir esta disciplina.");
            }

            _context.Disciplinas.Remove(disciplina);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<CadernetaDigital> CadernetaQuery()
        {
            return _context.CadernetasDigitais
                .Include(caderneta => caderneta.AlunoUsuario)
                .Include(caderneta => caderneta.ProfessorUsuario)
                .Include(caderneta => caderneta.TipoEnsino)
                .Include(caderneta => caderneta.TurmaEnsino)
                    .ThenInclude(turma => turma!.TipoEnsino)
                .Include(caderneta => caderneta.Disciplina)
                    .ThenInclude(disciplina => disciplina!.ProfessorUsuario)
                .Include(caderneta => caderneta.Disciplina)
                    .ThenInclude(disciplina => disciplina!.TurmaEnsino)
                    .ThenInclude(turma => turma!.TipoEnsino)
                .Include(caderneta => caderneta.Disciplina)
                    .ThenInclude(disciplina => disciplina!.AreaConhecimento)
                .AsNoTracking();
        }

        private async Task<Disciplina?> ObterDisciplinaParaLancamentoAsync(
            int disciplinaId,
            int tipoEnsinoId,
            int turmaEnsinoId,
            int professorUsuarioId)
        {
            return await _context.Disciplinas
                .Include(disciplina => disciplina.TurmaEnsino)
                .FirstOrDefaultAsync(disciplina =>
                    disciplina.IdDisciplina == disciplinaId
                    && disciplina.IdTurmaEnsino == turmaEnsinoId
                    && disciplina.TurmaEnsino!.IdTipoEnsino == tipoEnsinoId
                    && (disciplina.IdProfessorUsuario == null || disciplina.IdProfessorUsuario == professorUsuarioId));
        }

        private async Task<bool> DisciplinaJaExisteAsync(
            string nome,
            int idProfessorUsuario,
            int? idTurmaEnsino,
            int? ignorarDisciplinaId = null)
        {
            var nomeNormalizado = NormalizarNomeParaComparacao(nome);

            return await _context.Disciplinas
                .AnyAsync(disciplina =>
                    (!ignorarDisciplinaId.HasValue || disciplina.IdDisciplina != ignorarDisciplinaId.Value)
                    && disciplina.IdProfessorUsuario == idProfessorUsuario
                    && disciplina.IdTurmaEnsino == idTurmaEnsino
                    && disciplina.Nome.Trim().ToUpper() == nomeNormalizado);
        }

        private async Task ValidarEstruturaCurricularAsync(int? idTurmaEnsino, int? idAreaConhecimento)
        {
            if (!idTurmaEnsino.HasValue && !idAreaConhecimento.HasValue)
            {
                return;
            }

            TurmaEnsino? turma = null;
            AreaConhecimento? area = null;

            if (idTurmaEnsino.HasValue)
            {
                turma = await _context.TurmasEnsino
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.IdTurmaEnsino == idTurmaEnsino.Value);

                if (turma == null)
                {
                    throw new InvalidOperationException("Turma de ensino nao encontrada.");
                }
            }

            if (idAreaConhecimento.HasValue)
            {
                area = await _context.AreasConhecimento
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.IdAreaConhecimento == idAreaConhecimento.Value);

                if (area == null)
                {
                    throw new InvalidOperationException("Area de conhecimento nao encontrada.");
                }
            }

            if (turma != null && area != null && turma.IdTipoEnsino != area.IdTipoEnsino)
            {
                throw new InvalidOperationException("A area de conhecimento deve pertencer ao mesmo tipo de ensino da turma.");
            }
        }

        private async Task CriarNotificacaoLancamentoAsync(CadernetaDigitalViewModel caderneta, string operacao)
        {
            await _eventPublisher.PublishNotasPublicadasAsync(caderneta, operacao);
        }

        private async Task SaveChangesAsync(string duplicateMessage)
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new InvalidOperationException(duplicateMessage, ex);
            }
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException
                && sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
        }

        private static string NormalizarNomeParaComparacao(string nome)
        {
            return nome.Trim().ToUpperInvariant();
        }

        private static string? NormalizarTextoOpcional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private async Task<Usuario?> ObterAlunoAsync(int alunoUsuarioId)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(usuario => usuario.IdUsuario == alunoUsuarioId && usuario.IdPerfil == PerfilSistema.AlunoId);
        }

        private async Task ValidarMatriculaAlunoTurmaAsync(int alunoUsuarioId, int turmaEnsinoId)
        {
            var matricula = await _context.AlunosTurmasEnsino
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.IdAlunoUsuario == alunoUsuarioId);

            if (matricula == null)
            {
                throw new InvalidOperationException("Aluno nao matriculado em nenhuma turma.");
            }

            if (matricula.IdTurmaEnsino != turmaEnsinoId)
            {
                throw new InvalidOperationException("Aluno nao matriculado na turma informada.");
            }
        }

        private static bool PodeConsultar(ClaimsPrincipal principal, CadernetaDigital caderneta)
        {
            return IsAdministrador(principal)
                || (IsProfessor(principal) && PodeAdministrarLancamento(caderneta, GetUsuarioAtualId(principal)));
        }

        private static bool PodeAdministrarLancamento(CadernetaDigital caderneta, int usuarioId)
        {
            return caderneta.IdProfessorUsuario == usuarioId
                || caderneta.Disciplina?.IdProfessorUsuario == usuarioId;
        }

        private async Task<int> ValidarProfessorAsync(ClaimsPrincipal principal)
        {
            if (!IsProfessor(principal))
            {
                throw new UnauthorizedAccessException("Apenas professores podem administrar a caderneta digital.");
            }

            var usuarioId = GetUsuarioAtualId(principal);
            if (usuarioId <= 0)
            {
                throw new InvalidSessionException("Sessao invalida. Saia e entre novamente.");
            }

            var professorExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.IdUsuario == usuarioId && usuario.IdPerfil == PerfilSistema.ProfessorId);

            if (!professorExiste)
            {
                throw new InvalidSessionException("Sessao invalida. Saia e entre novamente.");
            }

            return usuarioId;
        }

        private static CadernetaDigitalViewModel ToViewModel(CadernetaDigital caderneta)
        {
            var notas = DeserializeNotas(caderneta.Notas);
            var media = CalcularMediaAritmetica(notas);
            var situacao = CalcularSituacao(media, caderneta.Faltas);

            return new CadernetaDigitalViewModel
            {
                IdCadernetaDigital = caderneta.IdCadernetaDigital,
                IdAlunoUsuario = caderneta.IdAlunoUsuario,
                NomeAluno = caderneta.AlunoUsuario?.Nome ?? string.Empty,
                EmailAluno = caderneta.AlunoUsuario?.Email ?? string.Empty,
                IdDisciplina = caderneta.IdDisciplina,
                NomeDisciplina = caderneta.Disciplina?.Nome ?? string.Empty,
                IdProfessorUsuario = caderneta.IdProfessorUsuario ?? caderneta.Disciplina?.IdProfessorUsuario,
                NomeProfessor = caderneta.ProfessorUsuario?.Nome ?? caderneta.Disciplina?.ProfessorUsuario?.Nome ?? string.Empty,
                IdTipoEnsino = caderneta.IdTipoEnsino ?? caderneta.Disciplina?.TurmaEnsino?.IdTipoEnsino,
                NomeTipoEnsino = caderneta.TipoEnsino?.Nome ?? caderneta.Disciplina?.TurmaEnsino?.TipoEnsino?.Nome,
                IdTurmaEnsino = caderneta.IdTurmaEnsino ?? caderneta.Disciplina?.IdTurmaEnsino,
                NomeTurmaEnsino = caderneta.TurmaEnsino?.Nome ?? caderneta.Disciplina?.TurmaEnsino?.Nome,
                IdAreaConhecimento = caderneta.Disciplina?.IdAreaConhecimento,
                NomeAreaConhecimento = caderneta.Disciplina?.AreaConhecimento?.Nome,
                Notas = notas,
                MediaAritmetica = media,
                Situacao = situacao.Label,
                CorSituacao = situacao.Cor,
                Presencas = caderneta.Presencas,
                Faltas = caderneta.Faltas
            };
        }

        private static DisciplinaViewModel ToViewModel(Disciplina disciplina)
        {
            return new DisciplinaViewModel
            {
                IdDisciplina = disciplina.IdDisciplina,
                Nome = disciplina.Nome,
                IdProfessorUsuario = disciplina.IdProfessorUsuario,
                NomeProfessor = disciplina.ProfessorUsuario?.Nome ?? string.Empty,
                IdTipoEnsino = disciplina.TurmaEnsino?.IdTipoEnsino,
                NomeTipoEnsino = disciplina.TurmaEnsino?.TipoEnsino?.Nome,
                IdTurmaEnsino = disciplina.IdTurmaEnsino,
                NomeTurmaEnsino = disciplina.TurmaEnsino?.Nome,
                IdAreaConhecimento = disciplina.IdAreaConhecimento,
                NomeAreaConhecimento = disciplina.AreaConhecimento?.Nome,
                Observacao = disciplina.Observacao,
                OfertaObrigatoria = disciplina.OfertaObrigatoria,
                MatriculaFacultativa = disciplina.MatriculaFacultativa,
                Ordem = disciplina.Ordem
            };
        }

        private static string SerializeNotas(decimal[] notas)
        {
            return string.Join(";", notas.Select(nota => nota.ToString(CultureInfo.InvariantCulture)));
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

        private static bool IsAdministrador(ClaimsPrincipal principal)
        {
            return principal.IsInRole(PerfilSistema.Administrador);
        }

        private static bool IsProfessor(ClaimsPrincipal principal)
        {
            return principal.IsInRole(PerfilSistema.Professor);
        }

        private static bool IsAluno(ClaimsPrincipal principal)
        {
            return principal.IsInRole(PerfilSistema.Aluno);
        }

        private static int GetUsuarioAtualId(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var idUsuario) ? idUsuario : 0;
        }
    }
}

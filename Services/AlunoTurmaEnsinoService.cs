using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Security;
using ESCOLA_API.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public class AlunoTurmaEnsinoService : IAlunoTurmaEnsinoService
    {
        private const string DuplicateAlunoMessage = "Este aluno ja esta matriculado em uma turma.";
        private readonly DataContext _context;

        public AlunoTurmaEnsinoService(DataContext context)
        {
            _context = context;
        }

        public async Task<AlunoTurmaEnsinoViewModel[]> GetAllAsync(ClaimsPrincipal principal)
        {
            var query = MatriculasQuery();

            if (IsAluno(principal))
            {
                var usuarioId = GetUsuarioAtualId(principal);
                query = query.Where(matricula => matricula.IdAlunoUsuario == usuarioId);
            }
            else if (!IsAdministrador(principal) && !IsProfessor(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar matriculas de alunos em turmas.");
            }

            var matriculas = await query
                .OrderBy(matricula => matricula.TurmaEnsino!.TipoEnsino!.Ordem)
                .ThenBy(matricula => matricula.TurmaEnsino!.Ordem)
                .ThenBy(matricula => matricula.AlunoUsuario!.Nome)
                .ToArrayAsync();

            return matriculas.Select(ToViewModel).ToArray();
        }

        public async Task<AlunoTurmaEnsinoViewModel?> GetByIdAsync(int matriculaId, ClaimsPrincipal principal)
        {
            var matricula = await MatriculasQuery()
                .FirstOrDefaultAsync(item => item.IdAlunoTurmaEnsino == matriculaId);

            if (matricula != null && !PodeConsultar(principal, matricula))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar esta matricula.");
            }

            return matricula == null ? null : ToViewModel(matricula);
        }

        public async Task<AlunoTurmaEnsinoViewModel> AddAsync(
            AlunoTurmaEnsinoCreateUpdateViewModel viewModel,
            ClaimsPrincipal principal)
        {
            ValidarAdministrador(principal);
            await ValidarAlunoAsync(viewModel.IdAlunoUsuario);
            await ValidarTurmaAsync(viewModel.IdTurmaEnsino);

            var jaMatriculado = await _context.AlunosTurmasEnsino
                .AnyAsync(matricula => matricula.IdAlunoUsuario == viewModel.IdAlunoUsuario);

            if (jaMatriculado)
            {
                throw new InvalidOperationException(DuplicateAlunoMessage);
            }

            var matricula = new AlunoTurmaEnsino
            {
                IdAlunoUsuario = viewModel.IdAlunoUsuario,
                IdTurmaEnsino = viewModel.IdTurmaEnsino,
                IdUsuarioResponsavel = GetUsuarioAtualId(principal),
                MatriculadoEmUtc = DateTime.UtcNow
            };

            _context.AlunosTurmasEnsino.Add(matricula);
            await SaveChangesAsync();

            return (await GetByIdAsync(matricula.IdAlunoTurmaEnsino, principal))!;
        }

        public async Task<AlunoTurmaEnsinoViewModel?> UpdateAsync(
            int matriculaId,
            AlunoTurmaEnsinoCreateUpdateViewModel viewModel,
            ClaimsPrincipal principal)
        {
            ValidarAdministrador(principal);
            var matricula = await _context.AlunosTurmasEnsino
                .FirstOrDefaultAsync(item => item.IdAlunoTurmaEnsino == matriculaId);

            if (matricula == null)
            {
                return null;
            }

            await ValidarAlunoAsync(viewModel.IdAlunoUsuario);
            await ValidarTurmaAsync(viewModel.IdTurmaEnsino);

            var jaMatriculado = await _context.AlunosTurmasEnsino
                .AnyAsync(item =>
                    item.IdAlunoTurmaEnsino != matriculaId
                    && item.IdAlunoUsuario == viewModel.IdAlunoUsuario);

            if (jaMatriculado)
            {
                throw new InvalidOperationException(DuplicateAlunoMessage);
            }

            if (matricula.IdAlunoUsuario != viewModel.IdAlunoUsuario || matricula.IdTurmaEnsino != viewModel.IdTurmaEnsino)
            {
                matricula.MatriculadoEmUtc = DateTime.UtcNow;
            }

            matricula.IdAlunoUsuario = viewModel.IdAlunoUsuario;
            matricula.IdTurmaEnsino = viewModel.IdTurmaEnsino;
            matricula.IdUsuarioResponsavel = GetUsuarioAtualId(principal);

            await SaveChangesAsync();
            return await GetByIdAsync(matriculaId, principal);
        }

        public async Task<bool> DeleteAsync(int matriculaId, ClaimsPrincipal principal)
        {
            ValidarAdministrador(principal);
            var matricula = await _context.AlunosTurmasEnsino
                .FirstOrDefaultAsync(item => item.IdAlunoTurmaEnsino == matriculaId);

            if (matricula == null)
            {
                return false;
            }

            _context.AlunosTurmasEnsino.Remove(matricula);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<AlunoTurmaEnsino> MatriculasQuery()
        {
            return _context.AlunosTurmasEnsino
                .Include(matricula => matricula.AlunoUsuario)
                .Include(matricula => matricula.TurmaEnsino)
                    .ThenInclude(turma => turma!.TipoEnsino)
                .Include(matricula => matricula.UsuarioResponsavel)
                .AsNoTracking();
        }

        private async Task ValidarAlunoAsync(int alunoUsuarioId)
        {
            var alunoExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.IdUsuario == alunoUsuarioId && usuario.IdPerfil == PerfilSistema.AlunoId);

            if (!alunoExiste)
            {
                throw new InvalidOperationException("Aluno nao encontrado.");
            }
        }

        private async Task ValidarTurmaAsync(int turmaEnsinoId)
        {
            var turmaExiste = await _context.TurmasEnsino
                .AnyAsync(turma => turma.IdTurmaEnsino == turmaEnsinoId);

            if (!turmaExiste)
            {
                throw new InvalidOperationException("Turma de ensino nao encontrada.");
            }
        }

        private async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new InvalidOperationException(DuplicateAlunoMessage, ex);
            }
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException switch
            {
                SqlException sqlException => sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627),
                SqliteException sqliteException => sqliteException.SqliteErrorCode == 19,
                _ => false
            };
        }

        private static AlunoTurmaEnsinoViewModel ToViewModel(AlunoTurmaEnsino matricula)
        {
            return new AlunoTurmaEnsinoViewModel
            {
                IdAlunoTurmaEnsino = matricula.IdAlunoTurmaEnsino,
                IdAlunoUsuario = matricula.IdAlunoUsuario,
                NomeAluno = matricula.AlunoUsuario?.Nome ?? string.Empty,
                EmailAluno = matricula.AlunoUsuario?.Email ?? string.Empty,
                IdTipoEnsino = matricula.TurmaEnsino?.IdTipoEnsino ?? 0,
                NomeTipoEnsino = matricula.TurmaEnsino?.TipoEnsino?.Nome ?? string.Empty,
                IdTurmaEnsino = matricula.IdTurmaEnsino,
                NomeTurmaEnsino = matricula.TurmaEnsino?.Nome ?? string.Empty,
                CodigoTurma = matricula.TurmaEnsino?.Codigo ?? string.Empty,
                IdUsuarioResponsavel = matricula.IdUsuarioResponsavel,
                NomeUsuarioResponsavel = matricula.UsuarioResponsavel?.Nome,
                MatriculadoEmUtc = matricula.MatriculadoEmUtc
            };
        }

        private static void ValidarAdministrador(ClaimsPrincipal principal)
        {
            if (!IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Apenas administradores podem administrar matriculas de alunos em turmas.");
            }
        }

        private static bool PodeConsultar(ClaimsPrincipal principal, AlunoTurmaEnsino matricula)
        {
            return IsAdministrador(principal)
                || IsProfessor(principal)
                || (IsAluno(principal) && matricula.IdAlunoUsuario == GetUsuarioAtualId(principal));
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

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

        public CadernetaDigitalService(DataContext context)
        {
            _context = context;
        }

        public async Task<CadernetaDigitalViewModel[]> GetAllAsync(ClaimsPrincipal principal)
        {
            var query = CadernetaQuery();

            if (IsAluno(principal))
            {
                var usuarioId = GetUsuarioAtualId(principal);
                query = query.Where(caderneta => caderneta.IdAlunoUsuario == usuarioId);
            }
            else if (IsProfessor(principal))
            {
                var usuarioId = GetUsuarioAtualId(principal);
                query = query.Where(caderneta => caderneta.Disciplina!.IdProfessorUsuario == usuarioId);
            }
            else if (!IsProfessor(principal) && !IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar a caderneta.");
            }

            var cadernetas = await query
                .OrderBy(caderneta => caderneta.AlunoUsuario!.Nome)
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
            ValidarProfessor(principal);
            var disciplina = await ObterDisciplinaDoProfessorAsync(viewModel.IdDisciplina, principal);
            var aluno = await ObterAlunoAsync(viewModel.IdAlunoUsuario);

            if (disciplina == null)
            {
                throw new InvalidOperationException("Disciplina nao encontrada para este professor.");
            }

            if (aluno == null)
            {
                throw new InvalidOperationException("Aluno nao encontrado.");
            }

            var jaAssociado = await _context.CadernetasDigitais
                .AnyAsync(caderneta => caderneta.IdAlunoUsuario == aluno.IdUsuario && caderneta.IdDisciplina == disciplina.IdDisciplina);

            if (jaAssociado)
            {
                throw new InvalidOperationException("Este aluno ja esta associado a esta disciplina.");
            }

            var caderneta = new CadernetaDigital
            {
                IdAlunoUsuario = aluno.IdUsuario,
                IdDisciplina = disciplina.IdDisciplina,
                Notas = SerializeNotas(viewModel.Notas),
                Presencas = viewModel.Presencas,
                Faltas = viewModel.Faltas
            };

            _context.CadernetasDigitais.Add(caderneta);
            await SaveChangesAsync("Este aluno ja esta associado a esta disciplina.");

            return (await GetByIdAsync(caderneta.IdCadernetaDigital, principal))!;
        }

        public async Task<CadernetaDigitalViewModel?> UpdateAsync(int cadernetaId, CadernetaDigitalCreateUpdateViewModel viewModel, ClaimsPrincipal principal)
        {
            ValidarProfessor(principal);
            var caderneta = await _context.CadernetasDigitais
                .Include(item => item.Disciplina)
                .FirstOrDefaultAsync(item => item.IdCadernetaDigital == cadernetaId);

            if (caderneta == null)
            {
                return null;
            }

            if (caderneta.Disciplina?.IdProfessorUsuario != GetUsuarioAtualId(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a alterar este lancamento.");
            }

            var disciplina = await ObterDisciplinaDoProfessorAsync(viewModel.IdDisciplina, principal);
            var aluno = await ObterAlunoAsync(viewModel.IdAlunoUsuario);

            if (disciplina == null)
            {
                throw new InvalidOperationException("Disciplina nao encontrada para este professor.");
            }

            if (aluno == null)
            {
                throw new InvalidOperationException("Aluno nao encontrado.");
            }

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
            caderneta.IdDisciplina = disciplina.IdDisciplina;
            caderneta.Notas = SerializeNotas(viewModel.Notas);
            caderneta.Presencas = viewModel.Presencas;
            caderneta.Faltas = viewModel.Faltas;

            await SaveChangesAsync("Este aluno ja esta associado a esta disciplina.");
            return await GetByIdAsync(cadernetaId, principal);
        }

        public async Task<bool> DeleteAsync(int cadernetaId, ClaimsPrincipal principal)
        {
            ValidarProfessor(principal);
            var caderneta = await _context.CadernetasDigitais
                .Include(item => item.Disciplina)
                .FirstOrDefaultAsync(item => item.IdCadernetaDigital == cadernetaId);

            if (caderneta == null)
            {
                return false;
            }

            if (caderneta.Disciplina?.IdProfessorUsuario != GetUsuarioAtualId(principal))
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
                .AsNoTracking();

            if (IsProfessor(principal))
            {
                var usuarioId = GetUsuarioAtualId(principal);
                query = query.Where(disciplina => disciplina.IdProfessorUsuario == usuarioId);
            }
            else if (IsAluno(principal))
            {
                var usuarioId = GetUsuarioAtualId(principal);
                query = query.Where(disciplina =>
                    disciplina.Cadernetas.Any(caderneta => caderneta.IdAlunoUsuario == usuarioId));
            }
            else if (!IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar disciplinas.");
            }

            var disciplinas = await query
                .OrderBy(disciplina => disciplina.Nome)
                .ToArrayAsync();

            return disciplinas.Select(ToViewModel).ToArray();
        }

        public async Task<DisciplinaViewModel> AddDisciplinaAsync(DisciplinaCreateUpdateViewModel viewModel, ClaimsPrincipal principal)
        {
            ValidarProfessor(principal);
            var usuarioId = GetUsuarioAtualId(principal);
            var nome = viewModel.Nome.Trim();

            var jaExiste = await DisciplinaJaExisteAsync(nome);

            if (jaExiste)
            {
                throw new InvalidOperationException("Disciplina ja cadastrada.");
            }

            var disciplina = new Disciplina
            {
                Nome = nome,
                IdProfessorUsuario = usuarioId
            };

            _context.Disciplinas.Add(disciplina);
            await SaveChangesAsync("Disciplina ja cadastrada.");

            var created = await _context.Disciplinas
                .Include(item => item.ProfessorUsuario)
                .AsNoTracking()
                .FirstAsync(item => item.IdDisciplina == disciplina.IdDisciplina);

            return ToViewModel(created);
        }

        public async Task<DisciplinaViewModel?> UpdateDisciplinaAsync(int disciplinaId, DisciplinaCreateUpdateViewModel viewModel, ClaimsPrincipal principal)
        {
            ValidarProfessor(principal);
            var usuarioId = GetUsuarioAtualId(principal);
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
            var jaExiste = await DisciplinaJaExisteAsync(nome, disciplinaId);

            if (jaExiste)
            {
                throw new InvalidOperationException("Disciplina ja cadastrada.");
            }

            disciplina.Nome = nome;
            await SaveChangesAsync("Disciplina ja cadastrada.");

            var updated = await _context.Disciplinas
                .Include(item => item.ProfessorUsuario)
                .AsNoTracking()
                .FirstAsync(item => item.IdDisciplina == disciplinaId);

            return ToViewModel(updated);
        }

        public async Task<bool> DeleteDisciplinaAsync(int disciplinaId, ClaimsPrincipal principal)
        {
            ValidarProfessor(principal);
            var usuarioId = GetUsuarioAtualId(principal);
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
                .Include(caderneta => caderneta.Disciplina)
                    .ThenInclude(disciplina => disciplina!.ProfessorUsuario)
                .AsNoTracking();
        }

        private async Task<Disciplina?> ObterDisciplinaDoProfessorAsync(int disciplinaId, ClaimsPrincipal principal)
        {
            var usuarioId = GetUsuarioAtualId(principal);
            return await _context.Disciplinas
                .FirstOrDefaultAsync(disciplina => disciplina.IdDisciplina == disciplinaId && disciplina.IdProfessorUsuario == usuarioId);
        }

        private async Task<bool> DisciplinaJaExisteAsync(string nome, int? ignorarDisciplinaId = null)
        {
            var nomeNormalizado = NormalizarNomeParaComparacao(nome);

            return await _context.Disciplinas
                .AnyAsync(disciplina =>
                    (!ignorarDisciplinaId.HasValue || disciplina.IdDisciplina != ignorarDisciplinaId.Value)
                    && disciplina.Nome.Trim().ToUpper() == nomeNormalizado);
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

        private async Task<Usuario?> ObterAlunoAsync(int alunoUsuarioId)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(usuario => usuario.IdUsuario == alunoUsuarioId && usuario.IdPerfil == PerfilSistema.AlunoId);
        }

        private static bool PodeConsultar(ClaimsPrincipal principal, CadernetaDigital caderneta)
        {
            return IsAdministrador(principal)
                || (IsProfessor(principal) && caderneta.Disciplina?.IdProfessorUsuario == GetUsuarioAtualId(principal))
                || (IsAluno(principal) && caderneta.IdAlunoUsuario == GetUsuarioAtualId(principal));
        }

        private static void ValidarProfessor(ClaimsPrincipal principal)
        {
            if (!IsProfessor(principal))
            {
                throw new UnauthorizedAccessException("Apenas professores podem administrar a caderneta digital.");
            }
        }

        private static CadernetaDigitalViewModel ToViewModel(CadernetaDigital caderneta)
        {
            return new CadernetaDigitalViewModel
            {
                IdCadernetaDigital = caderneta.IdCadernetaDigital,
                IdAlunoUsuario = caderneta.IdAlunoUsuario,
                NomeAluno = caderneta.AlunoUsuario?.Nome ?? string.Empty,
                EmailAluno = caderneta.AlunoUsuario?.Email ?? string.Empty,
                IdDisciplina = caderneta.IdDisciplina,
                NomeDisciplina = caderneta.Disciplina?.Nome ?? string.Empty,
                IdProfessorUsuario = caderneta.Disciplina?.IdProfessorUsuario ?? 0,
                NomeProfessor = caderneta.Disciplina?.ProfessorUsuario?.Nome ?? string.Empty,
                Notas = DeserializeNotas(caderneta.Notas),
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
                NomeProfessor = disciplina.ProfessorUsuario?.Nome ?? string.Empty
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

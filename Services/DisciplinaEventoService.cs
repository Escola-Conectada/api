using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Security;
using ESCOLA_API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public class DisciplinaEventoService : IDisciplinaEventoService
    {
        private readonly DataContext _context;

        public DisciplinaEventoService(DataContext context)
        {
            _context = context;
        }

        public async Task<DisciplinaEventoViewModel[]> GetEventosAsync(
            ClaimsPrincipal principal,
            int? idDisciplina,
            int? ano,
            int? mes)
        {
            ValidarPeriodo(ano, mes);

            var query = EventosQuery();

            if (IsProfessor(principal))
            {
                var usuarioId = GetUsuarioAtualId(principal);
                query = query.Where(evento => evento.Disciplina!.IdProfessorUsuario == usuarioId);
            }
            else if (IsAluno(principal))
            {
                var usuarioId = GetUsuarioAtualId(principal);
                query = query.Where(evento =>
                    evento.Disciplina!.Cadernetas.Any(caderneta => caderneta.IdAlunoUsuario == usuarioId));
            }
            else if (!IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar eventos de disciplinas.");
            }

            if (idDisciplina.HasValue)
            {
                query = query.Where(evento => evento.IdDisciplina == idDisciplina.Value);
            }

            if (ano.HasValue)
            {
                query = query.Where(evento => evento.Data.Year == ano.Value);
            }

            if (mes.HasValue)
            {
                query = query.Where(evento => evento.Data.Month == mes.Value);
            }

            var eventos = await query
                .OrderBy(evento => evento.Data)
                .ThenBy(evento => evento.Disciplina!.Nome)
                .ThenBy(evento => evento.Titulo)
                .ToArrayAsync();

            return eventos.Select(ToViewModel).ToArray();
        }

        public async Task<DisciplinaEventoViewModel> AddAsync(
            int disciplinaId,
            DisciplinaEventoCreateUpdateViewModel viewModel,
            ClaimsPrincipal principal)
        {
            var usuarioId = await ValidarProfessorAsync(principal);
            var disciplina = await ObterDisciplinaDoProfessorAsync(disciplinaId, usuarioId);

            if (disciplina == null)
            {
                throw new InvalidOperationException("Disciplina nao encontrada para este professor.");
            }

            var evento = new DisciplinaEvento
            {
                IdDisciplina = disciplina.IdDisciplina,
                Tipo = TipoEventoDisciplina.Normalizar(viewModel.Tipo),
                Titulo = viewModel.Titulo.Trim(),
                Descricao = NormalizarDescricao(viewModel.Descricao),
                Data = viewModel.Data,
                CriadoEmUtc = DateTime.UtcNow
            };

            _context.DisciplinaEventos.Add(evento);
            await _context.SaveChangesAsync();

            var created = await EventosQuery()
                .FirstAsync(item => item.IdEventoDisciplina == evento.IdEventoDisciplina);

            return ToViewModel(created);
        }

        public async Task<DisciplinaEventoViewModel?> UpdateAsync(
            int disciplinaId,
            int eventoId,
            DisciplinaEventoCreateUpdateViewModel viewModel,
            ClaimsPrincipal principal)
        {
            var usuarioId = await ValidarProfessorAsync(principal);
            var evento = await _context.DisciplinaEventos
                .Include(item => item.Disciplina)
                .FirstOrDefaultAsync(item => item.IdEventoDisciplina == eventoId && item.IdDisciplina == disciplinaId);

            if (evento == null)
            {
                return null;
            }

            if (evento.Disciplina?.IdProfessorUsuario != usuarioId)
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a alterar este evento.");
            }

            evento.Tipo = TipoEventoDisciplina.Normalizar(viewModel.Tipo);
            evento.Titulo = viewModel.Titulo.Trim();
            evento.Descricao = NormalizarDescricao(viewModel.Descricao);
            evento.Data = viewModel.Data;
            evento.AtualizadoEmUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updated = await EventosQuery()
                .FirstAsync(item => item.IdEventoDisciplina == eventoId);

            return ToViewModel(updated);
        }

        public async Task<bool> DeleteAsync(int disciplinaId, int eventoId, ClaimsPrincipal principal)
        {
            var usuarioId = await ValidarProfessorAsync(principal);
            var evento = await _context.DisciplinaEventos
                .Include(item => item.Disciplina)
                .FirstOrDefaultAsync(item => item.IdEventoDisciplina == eventoId && item.IdDisciplina == disciplinaId);

            if (evento == null)
            {
                return false;
            }

            if (evento.Disciplina?.IdProfessorUsuario != usuarioId)
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a excluir este evento.");
            }

            _context.DisciplinaEventos.Remove(evento);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<DisciplinaEvento> EventosQuery()
        {
            return _context.DisciplinaEventos
                .Include(evento => evento.Disciplina)
                    .ThenInclude(disciplina => disciplina!.ProfessorUsuario)
                .AsNoTracking();
        }

        private async Task<Disciplina?> ObterDisciplinaDoProfessorAsync(int disciplinaId, int usuarioId)
        {
            return await _context.Disciplinas
                .FirstOrDefaultAsync(disciplina =>
                    disciplina.IdDisciplina == disciplinaId
                    && disciplina.IdProfessorUsuario == usuarioId);
        }

        private async Task<int> ValidarProfessorAsync(ClaimsPrincipal principal)
        {
            if (!IsProfessor(principal))
            {
                throw new UnauthorizedAccessException("Apenas professores podem administrar eventos de disciplinas.");
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

        private static DisciplinaEventoViewModel ToViewModel(DisciplinaEvento evento)
        {
            return new DisciplinaEventoViewModel
            {
                IdEventoDisciplina = evento.IdEventoDisciplina,
                IdDisciplina = evento.IdDisciplina,
                NomeDisciplina = evento.Disciplina?.Nome ?? string.Empty,
                IdProfessorUsuario = evento.Disciplina?.IdProfessorUsuario ?? 0,
                NomeProfessor = evento.Disciplina?.ProfessorUsuario?.Nome ?? string.Empty,
                Tipo = evento.Tipo,
                Titulo = evento.Titulo,
                Descricao = evento.Descricao,
                Data = evento.Data,
                CriadoEmUtc = evento.CriadoEmUtc,
                AtualizadoEmUtc = evento.AtualizadoEmUtc
            };
        }

        private static void ValidarPeriodo(int? ano, int? mes)
        {
            if (ano.HasValue && (ano.Value < 1900 || ano.Value > 2100))
            {
                throw new InvalidOperationException("Informe um ano entre 1900 e 2100.");
            }

            if (mes.HasValue && (mes.Value < 1 || mes.Value > 12))
            {
                throw new InvalidOperationException("Informe um mes entre 1 e 12.");
            }
        }

        private static string? NormalizarDescricao(string? descricao)
        {
            return string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
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

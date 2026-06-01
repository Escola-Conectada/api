using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESCOLA_API.Models;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Data
{
    public class Repository : IRepository
    {
        public DataContext _context { get; }
        public Repository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Update<T>(T entity) where T : class
        {
            _context.Update(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        // Aluno
        public async Task<Aluno[]> GetAllAlunosAsync(bool includeProfessor = false)
        {
            IQueryable<Aluno> query = _context.Alunos;
            if (includeProfessor)
            {
                query = query
                .Include(a => a.Professor)
                .Include(a => a.Usuario)
                .ThenInclude(u => u!.Perfil);
            }

            query = query
            .AsNoTracking()
            .OrderBy(a => a.Id);

            return await query.ToArrayAsync();
        }

        public async Task<Aluno[]> GetAlunoAsyncByProfessorId(int ProfessorId, bool includeProfessor)
        {
            IQueryable<Aluno> query = _context.Alunos;
            if (includeProfessor)
            {
                query = query
                .Include(a => a.Professor)
                .Include(a => a.Usuario)
                .ThenInclude(u => u!.Perfil);
            }

            query = query
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Where(aluno => aluno.ProfessorId == ProfessorId);

            return await query.ToArrayAsync();
        }

        public async Task<Aluno?> GetAlunoAsyncById(int AlunoId, bool includeProfessor)
        {
            IQueryable<Aluno> query = _context.Alunos;
            if (includeProfessor)
            {
                query = query
                .Include(a => a.Professor)
                .Include(a => a.Usuario)
                .ThenInclude(u => u!.Perfil);
            }

            query = query
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Where(aluno => aluno.Id == AlunoId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> ProfessorExistsAsync(int professorId)
        {
            return await _context.Professores.AnyAsync(professor => professor.Id == professorId);
        }

        public async Task<bool> UsuarioExistsWithPerfilAsync(int usuarioId, int perfilId)
        {
            return await _context.Usuarios.AnyAsync(usuario =>
                usuario.IdUsuario == usuarioId
                && usuario.IdPerfil == perfilId);
        }

        public async Task<bool> AlunoUsuarioInUseAsync(int usuarioId, int? ignoreAlunoId = null)
        {
            return await _context.Alunos.AnyAsync(aluno =>
                aluno.IdUsuario == usuarioId
                && (!ignoreAlunoId.HasValue || aluno.Id != ignoreAlunoId.Value));
        }

        // Professor
        public async Task<Professor[]> GetAllProfessoresAsync(bool includeAluno = false)
        {
            IQueryable<Professor> query = _context.Professores;
            if (includeAluno)
            {
                query = query
                .Include(a => a.Alunos)
                .Include(a => a.Usuario)
                .ThenInclude(u => u!.Perfil);
            }

            query = query
            .AsNoTracking()
            .OrderBy(a => a.Id);

            return await query.ToArrayAsync();
        }

        public async Task<Professor?> GetProfessorAsyncById(int ProfessorId, bool includeAluno)
        {
            IQueryable<Professor> query = _context.Professores;
            if (includeAluno)
            {
                query = query
                .Include(a => a.Alunos)
                .Include(a => a.Usuario)
                .ThenInclude(u => u!.Perfil);
            }

            query = query
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Where(Professor => Professor.Id == ProfessorId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> ProfessorUsuarioInUseAsync(int usuarioId, int? ignoreProfessorId = null)
        {
            return await _context.Professores.AnyAsync(professor =>
                professor.IdUsuario == usuarioId
                && (!ignoreProfessorId.HasValue || professor.Id != ignoreProfessorId.Value));
        }

        // Diretoria
        public async Task<Diretoria[]> GetAllDiretoriasAsync(bool includeUsuario = false)
        {
            IQueryable<Diretoria> query = _context.Diretorias;
            if (includeUsuario)
            {
                query = query
                .Include(d => d.Usuario)
                .ThenInclude(u => u!.Perfil);
            }

            query = query
            .AsNoTracking()
            .OrderBy(d => d.Id);

            return await query.ToArrayAsync();
        }

        public async Task<Diretoria?> GetDiretoriaAsyncById(int DiretoriaId, bool includeUsuario)
        {
            IQueryable<Diretoria> query = _context.Diretorias;
            if (includeUsuario)
            {
                query = query
                .Include(d => d.Usuario)
                .ThenInclude(u => u!.Perfil);
            }

            query = query
            .AsNoTracking()
            .OrderBy(d => d.Id)
            .Where(diretoria => diretoria.Id == DiretoriaId);

            return await query.FirstOrDefaultAsync();
        }
    }
}

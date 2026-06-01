using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESCOLA_API.Models;

namespace ESCOLA_API.Data
{
    public interface IRepository
    {
        // Gerais
        void Add<T>(T entity) where T : class;

        void Update<T>(T entity) where T : class;

        void Delete<T>(T entity) where T : class;

        Task<bool> SaveChangesAsync();

        // Alunos
        Task<Aluno[]> GetAllAlunosAsync(bool includeProfessor);
        Task<Aluno[]> GetAlunoAsyncByProfessorId(int ProfessorId, bool includeProfessor);
        Task<Aluno?> GetAlunoAsyncById(int AlunoId, bool includeProfessor);
        Task<bool> ProfessorExistsAsync(int professorId);
        Task<bool> UsuarioExistsWithPerfilAsync(int usuarioId, int perfilId);
        Task<bool> AlunoUsuarioInUseAsync(int usuarioId, int? ignoreAlunoId = null);

        // Professor
        Task<Professor[]> GetAllProfessoresAsync(bool includeAluno);
        Task<Professor?> GetProfessorAsyncById(int ProfessorId, bool includeAluno);
        Task<bool> ProfessorUsuarioInUseAsync(int usuarioId, int? ignoreProfessorId = null);

        // Diretoria
        Task<Diretoria[]> GetAllDiretoriasAsync(bool includeUsuario);
        Task<Diretoria?> GetDiretoriaAsyncById(int DiretoriaId, bool includeUsuario);
    }
}

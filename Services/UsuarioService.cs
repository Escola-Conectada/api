using form_API.Data;
using form_API.Models;
using form_API.Security;
using form_API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace form_API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly DataContext _context;

        public UsuarioService(DataContext context)
        {
            _context = context;
        }

        public async Task<UsuarioSummaryViewModel[]> GetAllAsync()
        {
            return await _context.Usuarios
                .Include(usuario => usuario.Perfil)
                .AsNoTracking()
                .OrderBy(usuario => usuario.Nome)
                .Select(usuario => new UsuarioSummaryViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Telefone = usuario.Telefone,
                    IdPerfil = usuario.IdPerfil,
                    DescricaoPerfil = usuario.Perfil == null ? string.Empty : usuario.Perfil.DescricaoPerfil
                })
                .ToArrayAsync();
        }

        public async Task<UsuarioSummaryViewModel?> GetByIdAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);

            return usuario.ToSummary();
        }

        public async Task<UsuarioSummaryViewModel> AddAsync(UsuarioCreateViewModel viewModel)
        {
            var email = viewModel.Email.Trim().ToLowerInvariant();
            var emailJaCadastrado = await _context.Usuarios
                .AnyAsync(usuario => usuario.Email.ToLower() == email);

            if (emailJaCadastrado)
            {
                throw new InvalidOperationException("Email ja cadastrado.");
            }

            var perfilExiste = await _context.Perfis
                .AnyAsync(perfil => perfil.IdPerfil == viewModel.IdPerfil);

            if (!perfilExiste)
            {
                throw new InvalidOperationException("Perfil informado nao existe.");
            }

            var usuario = new Usuario
            {
                Nome = viewModel.Nome.Trim(),
                Email = email,
                Telefone = viewModel.Telefone.Trim(),
                Senha = PasswordHasher.HashPassword(viewModel.Senha),
                IdPerfil = viewModel.IdPerfil
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var created = await _context.Usuarios
                .Include(u => u.Perfil)
                .AsNoTracking()
                .FirstAsync(u => u.IdUsuario == usuario.IdUsuario);

            return created.ToSummary()!;
        }

        public async Task<UsuarioSummaryViewModel?> UpdateAsync(int usuarioId, UsuarioCreateViewModel viewModel)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);

            if (usuario == null)
            {
                return null;
            }

            var email = viewModel.Email.Trim().ToLowerInvariant();
            var emailJaCadastrado = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario != usuarioId && u.Email.ToLower() == email);

            if (emailJaCadastrado)
            {
                throw new InvalidOperationException("Email ja cadastrado.");
            }

            var perfilExiste = await _context.Perfis
                .AnyAsync(perfil => perfil.IdPerfil == viewModel.IdPerfil);

            if (!perfilExiste)
            {
                throw new InvalidOperationException("Perfil informado nao existe.");
            }

            usuario.Nome = viewModel.Nome.Trim();
            usuario.Email = email;
            usuario.Telefone = viewModel.Telefone.Trim();
            usuario.Senha = PasswordHasher.HashPassword(viewModel.Senha);
            usuario.IdPerfil = viewModel.IdPerfil;

            await _context.SaveChangesAsync();

            var updated = await _context.Usuarios
                .Include(u => u.Perfil)
                .AsNoTracking()
                .FirstAsync(u => u.IdUsuario == usuarioId);

            return updated.ToSummary();
        }

        public async Task<bool> DeleteAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);

            if (usuario == null)
            {
                return false;
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PerfilViewModel[]> GetPerfisAsync()
        {
            return await _context.Perfis
                .AsNoTracking()
                .OrderBy(perfil => perfil.IdPerfil)
                .Select(perfil => new PerfilViewModel
                {
                    IdPerfil = perfil.IdPerfil,
                    DescricaoPerfil = perfil.DescricaoPerfil
                })
                .ToArrayAsync();
        }
    }
}

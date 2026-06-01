using System.Security.Claims;
using ESCOLA_API.Data;
using ESCOLA_API.Models;
using ESCOLA_API.Security;
using ESCOLA_API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly DataContext _context;
        private readonly IConfiguracaoAplicacaoService? _configuracaoAplicacaoService;

        public UsuarioService(
            DataContext context,
            IConfiguracaoAplicacaoService? configuracaoAplicacaoService = null)
        {
            _context = context;
            _configuracaoAplicacaoService = configuracaoAplicacaoService;
        }

        public async Task<UsuarioSummaryViewModel[]> GetAllAsync(ClaimsPrincipal principal)
        {
            var query = _context.Usuarios
                .Include(usuario => usuario.Perfil)
                .AsNoTracking();

            if (IsProfessor(principal))
            {
                query = query.Where(usuario =>
                    usuario.IdPerfil == PerfilSistema.AlunoId || usuario.IdPerfil == PerfilSistema.ProfessorId);
            }
            else if (!IsAdministrador(principal))
            {
                var usuarioAtualId = GetUsuarioAtualId(principal);
                query = query.Where(usuario => usuario.IdUsuario == usuarioAtualId);
            }

            var usuarios = await query
                .OrderBy(usuario => usuario.Nome)
                .Select(usuario => new UsuarioSummaryViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Telefone = usuario.Telefone,
                    DataNascimento = usuario.DataNascimento,
                    NomeMae = usuario.NomeMae,
                    NomePai = usuario.NomePai,
                    Endereco = usuario.Endereco,
                    FotoPerfilUrl = usuario.FotoPerfilUrl,
                    IdPerfil = usuario.IdPerfil,
                    DescricaoPerfil = usuario.Perfil == null ? string.Empty : usuario.Perfil.DescricaoPerfil,
                    TipoUsuario = PerfilSistema.ObterDescricaoPorId(usuario.IdPerfil),
                    ExclusaoContaSolicitada = usuario.ExclusaoContaSolicitadaEmUtc.HasValue,
                    ExclusaoContaSolicitadaEmUtc = usuario.ExclusaoContaSolicitadaEmUtc
                })
                .ToArrayAsync();

            if (IsAdministrador(principal) || IsProfessor(principal))
            {
                await PreencherBoletinsDigitaisAsync(usuarios);
            }

            return usuarios;
        }

        public async Task<UsuarioSummaryViewModel?> GetByIdAsync(int usuarioId, ClaimsPrincipal principal)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);

            if (usuario != null && !PodeConsultar(principal, usuario))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar este cadastro.");
            }

            var viewModel = usuario.ToSummary();
            if (viewModel != null && IsAdministrador(principal))
            {
                await PreencherBoletinsDigitaisAsync([viewModel]);
            }

            return viewModel;
        }

        public async Task<UsuarioSummaryViewModel> AddAsync(UsuarioCreateViewModel viewModel, ClaimsPrincipal principal)
        {
            var email = NormalizeEmail(viewModel.Email);
            var emailJaCadastrado = await _context.Usuarios
                .AnyAsync(usuario => usuario.Email.ToLower() == email);

            if (emailJaCadastrado)
            {
                throw new InvalidOperationException("Email ja cadastrado.");
            }

            if (!PerfilSistema.TryObterPerfilId(viewModel.TipoUsuario, out var idPerfil))
            {
                throw new InvalidOperationException("Tipo de usuario informado nao existe.");
            }

            ValidarPermissaoCadastro(principal, idPerfil);

            var adminId = GetUsuarioAtualId(principal);
            var admin = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == adminId);

            var usuario = new Usuario
            {
                Nome = viewModel.Nome.Trim(),
                Email = email,
                Telefone = viewModel.Telefone.Trim(),
                DataNascimento = viewModel.DataNascimento,
                NomeMae = NormalizarTextoOpcional(viewModel.NomeMae),
                NomePai = NormalizarTextoOpcional(viewModel.NomePai),
                Endereco = NormalizarTextoOpcional(viewModel.Endereco),
                Senha = PasswordHasher.HashPassword(DefaultPasswordPolicy.DefaultPassword),
                IdPerfil = idPerfil,
                IdUsuarioCriador = admin?.IdUsuario,
                NomeUsuarioCriador = admin?.Nome
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            await CriarNotificacaoCadastroAsync(usuario);
            await _context.SaveChangesAsync();

            var created = await _context.Usuarios
                .Include(u => u.Perfil)
                .AsNoTracking()
                .FirstAsync(u => u.IdUsuario == usuario.IdUsuario);

            return created.ToSummary()!;
        }

        public async Task<UsuarioSummaryViewModel?> UpdateAsync(int usuarioId, UsuarioUpdateViewModel viewModel, ClaimsPrincipal principal)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);

            if (usuario == null)
            {
                return null;
            }

            var email = NormalizeEmail(viewModel.Email);
            var emailJaCadastrado = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario != usuarioId && u.Email.ToLower() == email);

            if (emailJaCadastrado)
            {
                throw new InvalidOperationException("Email ja cadastrado.");
            }

            var alterarTipoUsuario = !string.IsNullOrWhiteSpace(viewModel.TipoUsuario);
            var idPerfil = usuario.IdPerfil;

            if (alterarTipoUsuario && !PerfilSistema.TryObterPerfilId(viewModel.TipoUsuario, out idPerfil))
            {
                throw new InvalidOperationException("Tipo de usuario informado nao existe.");
            }

            ValidarPermissaoAtualizacao(principal, usuario, alterarTipoUsuario);

            var atualizacaoPropria = usuario.IdUsuario == GetUsuarioAtualId(principal) && !IsAdministrador(principal);
            var nomeMae = NormalizarTextoOpcional(viewModel.NomeMae);
            var nomePai = NormalizarTextoOpcional(viewModel.NomePai);
            var endereco = NormalizarTextoOpcional(viewModel.Endereco);
            var dadosAnteriores = new DadosPerfilUsuario(
                usuario.Nome,
                usuario.Email,
                usuario.Telefone,
                usuario.DataNascimento,
                usuario.NomeMae,
                usuario.NomePai,
                usuario.Endereco,
                usuario.IdPerfil);
            var dadosCadastraisAlterados =
                usuario.Nome != viewModel.Nome.Trim()
                || usuario.Email != email
                || usuario.Telefone != viewModel.Telefone.Trim()
                || usuario.DataNascimento != viewModel.DataNascimento
                || usuario.NomeMae != nomeMae
                || usuario.NomePai != nomePai
                || usuario.Endereco != endereco;

            usuario.Nome = viewModel.Nome.Trim();
            usuario.Email = email;
            usuario.Telefone = viewModel.Telefone.Trim();
            usuario.DataNascimento = viewModel.DataNascimento;
            usuario.NomeMae = nomeMae;
            usuario.NomePai = nomePai;
            usuario.Endereco = endereco;

            if (alterarTipoUsuario)
            {
                usuario.IdPerfil = idPerfil;
            }

            if (atualizacaoPropria && dadosCadastraisAlterados)
            {
                await CriarNotificacaoDadosAtualizadosAsync(usuario, dadosAnteriores);
            }

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

        public async Task<PerfilViewModel[]> GetPerfisAsync(ClaimsPrincipal principal)
        {
            if (!IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar perfis para cadastro.");
            }

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

        public async Task<ExclusaoContaSolicitadaViewModel?> SolicitarExclusaoContaAsync(
            ClaimsPrincipal principal,
            SolicitarExclusaoContaViewModel viewModel)
        {
            if (!viewModel.Confirmacao)
            {
                throw new InvalidOperationException("Confirme que deseja solicitar a exclusao da conta.");
            }

            var usuarioAtualId = GetUsuarioAtualId(principal);
            if (usuarioAtualId <= 0)
            {
                return null;
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioAtualId);

            if (usuario == null)
            {
                return null;
            }

            return await RegistrarSolicitacaoExclusaoContaAsync(usuario, viewModel.Motivo, "app");
        }

        public async Task<ExclusaoContaSolicitadaViewModel?> SolicitarExclusaoContaPorEmailAsync(
            SolicitarExclusaoContaPublicaViewModel viewModel)
        {
            var email = NormalizeEmail(viewModel.Email);
            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (usuario == null)
            {
                return null;
            }

            return await RegistrarSolicitacaoExclusaoContaAsync(usuario, viewModel.Motivo, "formulario publico");
        }

        public async Task<ExclusaoContaSolicitadaViewModel[]> GetSolicitacoesExclusaoContaAsync(ClaimsPrincipal principal)
        {
            if (!IsAdministrador(principal))
            {
                throw new UnauthorizedAccessException("Usuario nao autorizado a consultar solicitacoes de exclusao.");
            }

            return await _context.Usuarios
                .Include(usuario => usuario.Perfil)
                .AsNoTracking()
                .Where(usuario => usuario.ExclusaoContaSolicitadaEmUtc != null)
                .OrderBy(usuario => usuario.ExclusaoContaSolicitadaEmUtc)
                .Select(usuario => new ExclusaoContaSolicitadaViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    TipoUsuario = PerfilSistema.ObterDescricaoPorId(usuario.IdPerfil),
                    SolicitadaEmUtc = usuario.ExclusaoContaSolicitadaEmUtc!.Value,
                    Motivo = usuario.ExclusaoContaMotivo,
                    Status = "Pendente"
                })
                .ToArrayAsync();
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private static string? NormalizarTextoOpcional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static void ValidarPermissaoCadastro(ClaimsPrincipal principal, int idPerfil)
        {
            if (IsAdministrador(principal))
            {
                return;
            }

            throw new UnauthorizedAccessException("Usuario nao autorizado a cadastrar este tipo de usuario.");
        }

        private static void ValidarPermissaoAtualizacao(ClaimsPrincipal principal, Usuario usuario, bool alterarTipoUsuario)
        {
            if (IsAdministrador(principal))
            {
                return;
            }

            if (!alterarTipoUsuario && usuario.IdUsuario == GetUsuarioAtualId(principal))
            {
                return;
            }

            throw new UnauthorizedAccessException("Usuario nao autorizado a alterar este cadastro.");
        }

        private static bool PodeConsultar(ClaimsPrincipal principal, Usuario usuario)
        {
            if (IsAdministrador(principal))
            {
                return true;
            }

            var usuarioAtualId = GetUsuarioAtualId(principal);

            return usuario.IdUsuario == usuarioAtualId
                || (IsProfessor(principal)
                    && (usuario.IdPerfil == PerfilSistema.AlunoId || usuario.IdPerfil == PerfilSistema.ProfessorId));
        }

        private async Task CriarNotificacaoCadastroAsync(Usuario usuario)
        {
            var nomeCriador = string.IsNullOrWhiteSpace(usuario.NomeUsuarioCriador)
                ? "Administrador"
                : usuario.NomeUsuarioCriador;
            var nomeEscola = await GetNomeEscolaAsync();

            _context.Notificacoes.Add(new Notificacao
            {
                IdUsuario = usuario.IdUsuario,
                Tipo = "CadastroUsuario",
                Titulo = "Cadastro criado",
                Mensagem = $"Seu cadastro no {nomeEscola} foi criado por {nomeCriador}. Dados cadastrados: Nome: {usuario.Nome}; E-mail: {usuario.Email}; Telefone: {usuario.Telefone}; Data de nascimento: {FormatarDataNascimento(usuario.DataNascimento)}; Nome da mae: {FormatarValorOpcional(usuario.NomeMae)}; Nome do pai: {FormatarValorOpcional(usuario.NomePai)}; Endereco: {FormatarValorOpcional(usuario.Endereco)}; Perfil: {PerfilSistema.ObterDescricaoPorId(usuario.IdPerfil)}. Voce pode editar seus dados quando achar necessario.",
                Link = $"/usuarios/{usuario.IdUsuario}",
                CriadaEmUtc = DateTime.UtcNow
            });
        }

        private async Task CriarNotificacaoDadosAtualizadosAsync(Usuario usuario, DadosPerfilUsuario dadosAnteriores)
        {
            var administradoresIds = await _context.Usuarios
                .AsNoTracking()
                .Where(item => item.IdPerfil == PerfilSistema.AdministradorId)
                .Select(item => item.IdUsuario)
                .ToArrayAsync();

            if (administradoresIds.Length == 0)
            {
                return;
            }

            var mensagem = $"O usuario {usuario.Nome} alterou seus dados de perfil. "
                + $"Dados anteriores: Nome: {dadosAnteriores.Nome}; E-mail: {dadosAnteriores.Email}; Telefone: {dadosAnteriores.Telefone}; Data de nascimento: {FormatarDataNascimento(dadosAnteriores.DataNascimento)}; Nome da mae: {FormatarValorOpcional(dadosAnteriores.NomeMae)}; Nome do pai: {FormatarValorOpcional(dadosAnteriores.NomePai)}; Endereco: {FormatarValorOpcional(dadosAnteriores.Endereco)}; Perfil: {PerfilSistema.ObterDescricaoPorId(dadosAnteriores.IdPerfil)}. "
                + $"Dados atuais: Nome: {usuario.Nome}; E-mail: {usuario.Email}; Telefone: {usuario.Telefone}; Data de nascimento: {FormatarDataNascimento(usuario.DataNascimento)}; Nome da mae: {FormatarValorOpcional(usuario.NomeMae)}; Nome do pai: {FormatarValorOpcional(usuario.NomePai)}; Endereco: {FormatarValorOpcional(usuario.Endereco)}; Perfil: {PerfilSistema.ObterDescricaoPorId(usuario.IdPerfil)}.";

            foreach (var administradorId in administradoresIds)
            {
                _context.Notificacoes.Add(new Notificacao
                {
                    IdUsuario = administradorId,
                    Tipo = "DadosUsuarioAtualizados",
                    Titulo = "Dados do usuario atualizados",
                    Mensagem = mensagem,
                    Link = $"/usuarios/{usuario.IdUsuario}",
                    CriadaEmUtc = DateTime.UtcNow
                });
            }
        }

        private async Task<ExclusaoContaSolicitadaViewModel> RegistrarSolicitacaoExclusaoContaAsync(
            Usuario usuario,
            string? motivo,
            string origem)
        {
            usuario.ExclusaoContaSolicitadaEmUtc = DateTime.UtcNow;
            usuario.ExclusaoContaMotivo = TruncarTexto(NormalizarTextoOpcional(motivo), 500);

            await NotificarAdministradoresExclusaoContaAsync(usuario, origem);
            await _context.SaveChangesAsync();

            return new ExclusaoContaSolicitadaViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nome = usuario.Nome,
                Email = usuario.Email,
                TipoUsuario = PerfilSistema.ObterDescricaoPorId(usuario.IdPerfil),
                SolicitadaEmUtc = usuario.ExclusaoContaSolicitadaEmUtc.Value,
                Motivo = usuario.ExclusaoContaMotivo,
                Status = "Pendente"
            };
        }

        private async Task NotificarAdministradoresExclusaoContaAsync(Usuario usuario, string origem)
        {
            var administradoresIds = await _context.Usuarios
                .AsNoTracking()
                .Where(item => item.IdPerfil == PerfilSistema.AdministradorId)
                .Select(item => item.IdUsuario)
                .ToArrayAsync();

            foreach (var administradorId in administradoresIds)
            {
                _context.Notificacoes.Add(new Notificacao
                {
                    IdUsuario = administradorId,
                    Tipo = "ExclusaoContaSolicitada",
                    Titulo = "Solicitacao de exclusao de conta",
                    Mensagem = $"O usuario {usuario.Nome} ({usuario.Email}) solicitou exclusao da conta pelo {origem}. Motivo: {FormatarValorOpcional(usuario.ExclusaoContaMotivo)}.",
                    Link = "/usuarios/exclusoes-conta",
                    CriadaEmUtc = DateTime.UtcNow
                });
            }
        }

        private static string? TruncarTexto(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value[..maxLength];
        }

        private static bool IsAdministrador(ClaimsPrincipal principal)
        {
            return principal.IsInRole(PerfilSistema.Administrador);
        }

        private async Task<string> GetNomeEscolaAsync()
        {
            return _configuracaoAplicacaoService == null
                ? ConfiguracaoAplicacaoService.NomeEscolaPadrao
                : await _configuracaoAplicacaoService.GetNomeEscolaAsync();
        }

        private static bool IsProfessor(ClaimsPrincipal principal)
        {
            return principal.IsInRole(PerfilSistema.Professor);
        }

        private async Task PreencherBoletinsDigitaisAsync(UsuarioSummaryViewModel[] usuarios)
        {
            var alunos = usuarios
                .Where(usuario => usuario.IdPerfil == PerfilSistema.AlunoId)
                .ToArray();

            foreach (var aluno in alunos)
            {
                aluno.BoletimDigital = await CriarResumoBoletimDigitalAsync(aluno);
            }
        }

        private async Task<BoletimDigitalResumoAlunoViewModel?> CriarResumoBoletimDigitalAsync(UsuarioSummaryViewModel aluno)
        {
            var matricula = await _context.AlunosTurmasEnsino
                .Include(item => item.TurmaEnsino)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.IdAlunoUsuario == aluno.IdUsuario);

            if (matricula?.TurmaEnsino == null)
            {
                return null;
            }

            var disciplinasIds = await _context.Disciplinas
                .AsNoTracking()
                .Where(disciplina => disciplina.IdTurmaEnsino == matricula.IdTurmaEnsino)
                .Select(disciplina => disciplina.IdDisciplina)
                .ToArrayAsync();

            var lancadas = disciplinasIds.Length == 0
                ? 0
                : await _context.CadernetasDigitais
                    .AsNoTracking()
                    .Where(caderneta =>
                        caderneta.IdAlunoUsuario == aluno.IdUsuario
                        && caderneta.IdTurmaEnsino == matricula.IdTurmaEnsino
                        && disciplinasIds.Contains(caderneta.IdDisciplina))
                    .Select(caderneta => caderneta.IdDisciplina)
                    .Distinct()
                    .CountAsync();

            var boletim = await _context.BoletinsDigitais
                .Include(item => item.ProfessorSolicitanteUsuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.IdAlunoUsuario == aluno.IdUsuario
                    && item.IdTurmaEnsino == matricula.IdTurmaEnsino);

            var status = boletim?.Status ?? BoletimDigitalStatus.EmAberto;
            var completo = disciplinasIds.Length > 0 && lancadas == disciplinasIds.Length;
            var liberado = completo && status == BoletimDigitalStatus.Liberado;
            var pendenteLiberacao = completo && status == BoletimDigitalStatus.PendenteDiretoria;

            return new BoletimDigitalResumoAlunoViewModel
            {
                IdBoletimDigital = boletim?.IdBoletimDigital,
                IdAlunoUsuario = aluno.IdUsuario,
                NomeAluno = aluno.Nome,
                EmailAluno = aluno.Email,
                IdTurmaEnsino = matricula.IdTurmaEnsino,
                NomeTurmaEnsino = matricula.TurmaEnsino.Nome,
                Status = status,
                Completo = completo,
                Liberado = liberado,
                PendenteLiberacao = pendenteLiberacao,
                TotalDisciplinas = disciplinasIds.Length,
                DisciplinasLancadas = lancadas,
                DisciplinasPendentes = disciplinasIds.Length - lancadas,
                SolicitadoEmUtc = boletim?.SolicitadoEmUtc,
                NomeProfessorSolicitante = boletim?.ProfessorSolicitanteUsuario?.Nome ?? string.Empty
            };
        }

        private static int GetUsuarioAtualId(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var idUsuario) ? idUsuario : 0;
        }

        private static string FormatarDataNascimento(DateOnly? dataNascimento)
        {
            return dataNascimento?.ToString("dd/MM/yyyy") ?? "nao informada";
        }

        private static string FormatarValorOpcional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "nao informado" : value.Trim();
        }

        private sealed record DadosPerfilUsuario(
            string Nome,
            string Email,
            string Telefone,
            DateOnly? DataNascimento,
            string? NomeMae,
            string? NomePai,
            string? Endereco,
            int IdPerfil);
    }
}

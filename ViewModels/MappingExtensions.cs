using System.Linq;
using ESCOLA_API.Models;

namespace ESCOLA_API.ViewModels
{
    public static class MappingExtensions
    {
        public static AlunoViewModel ToViewModel(this Aluno entity)
        {
            return new AlunoViewModel
            {
                Id = entity.Id,
                Nome = entity.Nome,
                Sobrenome = entity.Sobrenome,
                DataNasc = entity.DataNasc,
                ProfessorId = entity.ProfessorId,
                Professor = entity.Professor == null ? null : new ProfessorSummaryViewModel
                {
                    Id = entity.Professor.Id,
                    Nome = entity.Professor.Nome,
                    IdUsuario = entity.Professor.IdUsuario
                },
                IdUsuario = entity.IdUsuario,
                Usuario = entity.Usuario.ToSummary()
            };
        }

        public static AlunoSummaryViewModel ToSummary(this Aluno entity)
        {
            return new AlunoSummaryViewModel
            {
                Id = entity.Id,
                Nome = entity.Nome,
                Sobrenome = entity.Sobrenome,
                ProfessorId = entity.ProfessorId,
                IdUsuario = entity.IdUsuario
            };
        }

        public static ProfessorViewModel ToViewModel(this Professor entity)
        {
            return new ProfessorViewModel
            {
                Id = entity.Id,
                Nome = entity.Nome,
                IdUsuario = entity.IdUsuario,
                Usuario = entity.Usuario.ToSummary(),
                Alunos = entity.Alunos?.Select(a => a.ToSummary()).ToArray() ?? new AlunoSummaryViewModel[0]
            };
        }

        public static DiretoriaViewModel ToViewModel(this Diretoria entity)
        {
            return new DiretoriaViewModel
            {
                Id = entity.Id,
                Nome = entity.Nome,
                IdUsuario = entity.IdUsuario,
                Usuario = entity.Usuario.ToSummary()
            };
        }

        public static UsuarioSummaryViewModel? ToSummary(this Usuario? entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new UsuarioSummaryViewModel
            {
                IdUsuario = entity.IdUsuario,
                Nome = entity.Nome,
                Email = entity.Email,
                Telefone = entity.Telefone,
                DataNascimento = entity.DataNascimento,
                NomeMae = entity.NomeMae,
                NomePai = entity.NomePai,
                Endereco = entity.Endereco,
                FotoPerfilUrl = entity.FotoPerfilUrl,
                IdPerfil = entity.IdPerfil,
                DescricaoPerfil = entity.Perfil?.DescricaoPerfil ?? string.Empty,
                TipoUsuario = PerfilSistema.ObterDescricaoPorId(entity.IdPerfil)
            };
        }

        public static UsuarioArquivoViewModel ToViewModel(this UsuarioArquivo entity)
        {
            return new UsuarioArquivoViewModel
            {
                IdArquivo = entity.IdArquivo,
                IdUsuario = entity.IdUsuario,
                NomeBlob = entity.NomeBlob,
                TipoArquivo = entity.TipoArquivo,
                NomeOriginal = entity.NomeOriginal,
                Url = entity.Url,
                ContentType = entity.ContentType,
                TamanhoBytes = entity.TamanhoBytes,
                CriadoEmUtc = entity.CriadoEmUtc
            };
        }

        public static NotificacaoViewModel ToViewModel(this Notificacao entity)
        {
            return new NotificacaoViewModel
            {
                IdNotificacao = entity.IdNotificacao,
                IdUsuario = entity.IdUsuario,
                Tipo = entity.Tipo,
                Titulo = entity.Titulo,
                Mensagem = entity.Mensagem,
                Link = entity.Link,
                IdCadernetaDigital = entity.IdCadernetaDigital,
                IdDisciplina = entity.IdDisciplina,
                NomeDisciplina = entity.NomeDisciplina,
                MediaAritmetica = entity.MediaAritmetica,
                Situacao = entity.Situacao,
                CorSituacao = entity.CorSituacao,
                Lida = entity.Lida,
                CriadaEmUtc = entity.CriadaEmUtc,
                LidaEmUtc = entity.LidaEmUtc
            };
        }

        public static Aluno ToModel(this AlunoCreateEditViewModel viewModel)
        {
            return new Aluno
            {
                Nome = viewModel.Nome,
                Sobrenome = viewModel.Sobrenome,
                DataNasc = viewModel.DataNasc,
                ProfessorId = viewModel.ProfessorId,
                IdUsuario = viewModel.IdUsuario
            };
        }

        public static void UpdateFrom(this Aluno entity, AlunoCreateEditViewModel viewModel)
        {
            entity.Nome = viewModel.Nome;
            entity.Sobrenome = viewModel.Sobrenome;
            entity.DataNasc = viewModel.DataNasc;
            entity.ProfessorId = viewModel.ProfessorId;
            entity.IdUsuario = viewModel.IdUsuario;
        }

        public static Professor ToModel(this ProfessorCreateEditViewModel viewModel)
        {
            return new Professor
            {
                Nome = viewModel.Nome,
                IdUsuario = viewModel.IdUsuario
            };
        }

        public static Diretoria ToModel(this DiretoriaCreateEditViewModel viewModel)
        {
            return new Diretoria
            {
                Nome = viewModel.Nome,
                IdUsuario = viewModel.IdUsuario
            };
        }

        public static void UpdateFrom(this Diretoria entity, DiretoriaCreateEditViewModel viewModel)
        {
            entity.Nome = viewModel.Nome;
            entity.IdUsuario = viewModel.IdUsuario;
        }
    }
}

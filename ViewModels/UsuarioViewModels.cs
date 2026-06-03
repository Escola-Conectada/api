using Microsoft.AspNetCore.Http;

namespace ESCOLA_API.ViewModels
{
    /// <summary>
    /// Dados para criacao de usuario.
    /// </summary>
    public class UsuarioCreateViewModel
    {
        /// <summary>
        /// Nome do usuario.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email usado para login.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefone de contato.
        /// </summary>
        public string Telefone { get; set; } = string.Empty;

        /// <summary>
        /// Data de nascimento do usuario.
        /// </summary>
        public DateOnly? DataNascimento { get; set; }

        /// <summary>
        /// Nome da mae do usuario.
        /// </summary>
        public string? NomeMae { get; set; }

        /// <summary>
        /// Nome do pai do usuario.
        /// </summary>
        public string? NomePai { get; set; }

        /// <summary>
        /// Endereco residencial do usuario.
        /// </summary>
        public string? Endereco { get; set; }

        /// <summary>
        /// Tipo do usuario: Aluno, Professor ou Administrador.
        /// </summary>
        public string TipoUsuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dados para atualizacao de usuario.
    /// </summary>
    public class UsuarioUpdateViewModel
    {
        /// <summary>
        /// Nome do usuario.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email usado para login.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefone de contato.
        /// </summary>
        public string Telefone { get; set; } = string.Empty;

        /// <summary>
        /// Data de nascimento do usuario.
        /// </summary>
        public DateOnly? DataNascimento { get; set; }

        /// <summary>
        /// Nome da mae do usuario.
        /// </summary>
        public string? NomeMae { get; set; }

        /// <summary>
        /// Nome do pai do usuario.
        /// </summary>
        public string? NomePai { get; set; }

        /// <summary>
        /// Endereco residencial do usuario.
        /// </summary>
        public string? Endereco { get; set; }

        /// <summary>
        /// URL publica da foto de perfil do usuario.
        /// </summary>
        public string? FotoPerfilUrl { get; set; }

        /// <summary>
        /// Tipo do usuario: Aluno, Professor ou Administrador. Apenas administradores podem alterar.
        /// </summary>
        public string? TipoUsuario { get; set; }
    }

    /// <summary>
    /// Dados publicos de um usuario, sem expor a senha.
    /// </summary>
    public class UsuarioSummaryViewModel
    {
        /// <summary>
        /// Identificador do usuario.
        /// </summary>
        public int IdUsuario { get; set; }

        /// <summary>
        /// Nome do usuario.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email usado para login.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefone de contato.
        /// </summary>
        public string Telefone { get; set; } = string.Empty;

        /// <summary>
        /// Data de nascimento do usuario.
        /// </summary>
        public DateOnly? DataNascimento { get; set; }

        /// <summary>
        /// Nome da mae do usuario.
        /// </summary>
        public string? NomeMae { get; set; }

        /// <summary>
        /// Nome do pai do usuario.
        /// </summary>
        public string? NomePai { get; set; }

        /// <summary>
        /// Endereco residencial do usuario.
        /// </summary>
        public string? Endereco { get; set; }

        /// <summary>
        /// URL publica da foto de perfil do usuario.
        /// </summary>
        public string? FotoPerfilUrl { get; set; }

        /// <summary>
        /// Identificador do perfil de autorizacao.
        /// </summary>
        public int IdPerfil { get; set; }

        /// <summary>
        /// Descricao do perfil de autorizacao.
        /// </summary>
        public string DescricaoPerfil { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do usuario.
        /// </summary>
        public string TipoUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o usuario solicitou exclusao da conta.
        /// </summary>
        public bool ExclusaoContaSolicitada { get; set; }

        /// <summary>
        /// Data e hora UTC da solicitacao de exclusao da conta.
        /// </summary>
        public DateTime? ExclusaoContaSolicitadaEmUtc { get; set; }

        /// <summary>
        /// Resumo do boletim digital quando o usuario e aluno e o solicitante tem permissao de consulta.
        /// </summary>
        public BoletimDigitalResumoAlunoViewModel? BoletimDigital { get; set; }
    }

    /// <summary>
    /// Dados enviados pelo usuario autenticado para solicitar exclusao da propria conta.
    /// </summary>
    public class SolicitarExclusaoContaViewModel
    {
        /// <summary>
        /// Confirmacao explicita de que o usuario deseja solicitar a exclusao da conta.
        /// </summary>
        public bool Confirmacao { get; set; }

        /// <summary>
        /// Motivo opcional informado pelo usuario.
        /// </summary>
        public string? Motivo { get; set; }
    }

    /// <summary>
    /// Dados enviados por formulario publico para solicitar exclusao da conta fora do app.
    /// </summary>
    public class SolicitarExclusaoContaPublicaViewModel
    {
        /// <summary>
        /// Email cadastrado na conta.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Motivo opcional informado pelo usuario.
        /// </summary>
        public string? Motivo { get; set; }
    }

    /// <summary>
    /// Solicitacao de exclusao de conta pendente de analise administrativa.
    /// </summary>
    public class ExclusaoContaSolicitadaViewModel
    {
        /// <summary>
        /// Identificador do usuario.
        /// </summary>
        public int IdUsuario { get; set; }

        /// <summary>
        /// Nome do usuario.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuario.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do usuario: Aluno, Professor ou Administrador.
        /// </summary>
        public string TipoUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora UTC da solicitacao.
        /// </summary>
        public DateTime SolicitadaEmUtc { get; set; }

        /// <summary>
        /// Motivo opcional informado na solicitacao.
        /// </summary>
        public string? Motivo { get; set; }

        /// <summary>
        /// Status atual da solicitacao.
        /// </summary>
        public string Status { get; set; } = "Pendente";
    }

    public class UsuarioArquivoViewModel
    {
        public int IdArquivo { get; set; }
        public int? IdUsuario { get; set; }
        public string? NomeBlob { get; set; }
        public string? TipoArquivo { get; set; }
        public string? NomeOriginal { get; set; }
        public string? Url { get; set; }
        public string? ContentType { get; set; }
        public long? TamanhoBytes { get; set; }
        public DateTime? CriadoEmUtc { get; set; }
    }

    /// <summary>
    /// Dados do formulario para envio de arquivo vinculado ao usuario.
    /// </summary>
    public class UsuarioArquivoUploadViewModel
    {
        /// <summary>
        /// Arquivo enviado pelo formulario.
        /// </summary>
        public IFormFile? Arquivo { get; set; }
    }

    /// <summary>
    /// Perfil de autorizacao disponivel para usuarios.
    /// </summary>
    public class PerfilViewModel
    {
        /// <summary>
        /// Identificador do perfil.
        /// </summary>
        public int IdPerfil { get; set; }

        /// <summary>
        /// Descricao do perfil.
        /// </summary>
        public string DescricaoPerfil { get; set; } = string.Empty;
    }
}

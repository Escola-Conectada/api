namespace ESCOLA_API.Models
{
    public static class PerfilSistema
    {
        public const int AdministradorId = 1;
        public const int ProfessorId = 2;
        public const int AlunoId = 3;

        public const string Administrador = "Administrador";
        public const string Professor = "Professor";
        public const string Aluno = "Aluno";

        private static readonly IReadOnlyDictionary<string, int> TipoUsuarioParaPerfil =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                [Administrador] = AdministradorId,
                ["Diretoria"] = AdministradorId,
                ["Membro da Diretoria"] = AdministradorId,
                ["Membro da Diretoria / Administrador"] = AdministradorId,
                [Professor] = ProfessorId,
                [Aluno] = AlunoId
            };

        public static bool TryObterPerfilId(string? tipoUsuario, out int idPerfil)
        {
            idPerfil = 0;

            return !string.IsNullOrWhiteSpace(tipoUsuario)
                && TipoUsuarioParaPerfil.TryGetValue(tipoUsuario.Trim(), out idPerfil);
        }

        public static bool TipoUsuarioValido(string? tipoUsuario)
        {
            return TryObterPerfilId(tipoUsuario, out _);
        }

        public static string ObterDescricaoPorId(int idPerfil)
        {
            return idPerfil switch
            {
                AdministradorId => Administrador,
                ProfessorId => Professor,
                AlunoId => Aluno,
                _ => string.Empty
            };
        }
    }
}

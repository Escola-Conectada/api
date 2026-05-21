namespace form_API.Security
{
    public static class DefaultPasswordPolicy
    {
        public const string DefaultPassword = "Senha@252525";

        public static bool UsesDefaultPassword(string storedHash)
        {
            return PasswordHasher.VerifyPassword(DefaultPassword, storedHash);
        }
    }
}

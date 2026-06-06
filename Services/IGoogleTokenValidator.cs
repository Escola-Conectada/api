namespace ESCOLA_API.Services
{
    public sealed record GoogleTokenPayload(string Email, bool EmailVerified, string? Name, string? Subject);

    public interface IGoogleTokenValidator
    {
        Task<GoogleTokenPayload?> ValidateAsync(string idToken, string clientId);
    }
}

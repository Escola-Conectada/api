using Google.Apis.Auth;

namespace ESCOLA_API.Services
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        public async Task<GoogleTokenPayload?> ValidateAsync(string idToken, string clientId)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });

                return new GoogleTokenPayload(
                    payload.Email,
                    payload.EmailVerified,
                    payload.Name,
                    payload.Subject);
            }
            catch (InvalidJwtException)
            {
                return null;
            }
        }
    }
}

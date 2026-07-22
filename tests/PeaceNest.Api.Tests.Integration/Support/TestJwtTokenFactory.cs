using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace PeaceNest.Api.Tests.Integration.Support;

public static class TestJwtTokenFactory
{
    public static string CreateSupabaseAccessToken(
        string subject = "123e4567-e89b-12d3-a456-426614174000",
        string email = "parent@example.test",
        string role = "authenticated",
        string provider = "google",
        bool includeCompletedProfile = true)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestingApiFactory.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = TestingApiFactory.Issuer,
            Audience = TestingApiFactory.Audience,
            Expires = DateTime.UtcNow.AddMinutes(10),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = credentials,
            Subject = new ClaimsIdentity(
                BuildClaims(subject, email, role, provider, includeCompletedProfile))
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static IReadOnlyCollection<Claim> BuildClaims(
        string subject,
        string email,
        string role,
        string provider,
        bool includeCompletedProfile)
    {
        var claims = new List<Claim>
        {
                    new Claim("sub", subject),
                    new Claim("email", email),
                    new Claim("role", role),
                    new Claim("app_metadata", $$"""{"provider":"{{provider}}","providers":["{{provider}}"]}""", JsonClaimValueTypes.Json),
                    new Claim("session_id", Guid.NewGuid().ToString())
        };

        if (includeCompletedProfile)
        {
            claims.Add(new Claim(
                "user_metadata",
                $$"""{"full_name":"{{email.Split('@')[0]}}","country_code":"PH"}""",
                JsonClaimValueTypes.Json));
        }

        return claims;
    }
}

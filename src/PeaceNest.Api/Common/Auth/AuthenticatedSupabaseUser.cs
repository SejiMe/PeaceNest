using System.Security.Claims;
using System.Text.Json;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Common.Auth;

public sealed record AuthenticatedSupabaseUser(
    Guid SupabaseUserId,
    string Email,
    string Provider);

public static class AuthenticatedSupabaseUserFactory
{
    public static AuthenticatedSupabaseUser FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        var subject = principal.FindFirst(AuthClaimTypes.Subject)?.Value;
        if (!Guid.TryParse(subject, out var supabaseUserId))
        {
            throw new AuthenticationAppException("A valid Supabase user subject is required.");
        }

        var email = principal.FindFirst(AuthClaimTypes.Email)?.Value?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new AuthenticationAppException("A verified Google email is required.");
        }

        var provider = ReadProvider(principal);
        if (!string.Equals(provider, "google", StringComparison.OrdinalIgnoreCase))
        {
            throw new AuthenticationAppException("PeaceNest supports Google sign-in only.");
        }

        return new AuthenticatedSupabaseUser(supabaseUserId, email, provider);
    }

    private static string ReadProvider(ClaimsPrincipal principal)
    {
        var appMetadata = principal.FindFirst(AuthClaimTypes.AppMetadata)?.Value;
        if (string.IsNullOrWhiteSpace(appMetadata))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(appMetadata);
            if (document.RootElement.TryGetProperty("provider", out var provider) &&
                provider.ValueKind == JsonValueKind.String)
            {
                return provider.GetString() ?? string.Empty;
            }
        }
        catch (JsonException)
        {
            throw new AuthenticationAppException("Supabase app metadata could not be read.");
        }

        return string.Empty;
    }
}

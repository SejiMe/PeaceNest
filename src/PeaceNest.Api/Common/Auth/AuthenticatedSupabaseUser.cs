using System.Security.Claims;
using System.Text.Json;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.Localization;

namespace PeaceNest.Api.Common.Auth;

public sealed record AuthenticatedSupabaseUser(
    Guid SupabaseUserId,
    string Email,
    string Provider,
    string? SuggestedDisplayName,
    string? AvatarUrl,
    string? CountryCode);

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

        var metadata = ReadUserMetadata(principal);
        return new AuthenticatedSupabaseUser(
            supabaseUserId,
            email,
            provider,
            metadata.DisplayName,
            metadata.AvatarUrl,
            metadata.CountryCode);
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

    private static UserMetadata ReadUserMetadata(ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(AuthClaimTypes.UserMetadata)?.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return new UserMetadata(null, null, null);
        }

        try
        {
            using var document = JsonDocument.Parse(value);
            var root = document.RootElement;
            var displayName = ReadString(root, "full_name") ?? ReadString(root, "name");
            var avatarUrl = ReadString(root, "avatar_url") ?? ReadString(root, "picture");
            var countryCode = ReadString(root, "country_code");

            return new UserMetadata(
                NormalizeOptional(displayName, 200),
                NormalizeOptional(avatarUrl, 2048),
                CountryRegions.IsSupported(countryCode) ? CountryRegions.Normalize(countryCode!) : null);
        }
        catch (JsonException)
        {
            return new UserMetadata(null, null, null);
        }
    }

    private static string? ReadString(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized[..Math.Min(normalized.Length, maxLength)];
    }

    private sealed record UserMetadata(string? DisplayName, string? AvatarUrl, string? CountryCode);
}

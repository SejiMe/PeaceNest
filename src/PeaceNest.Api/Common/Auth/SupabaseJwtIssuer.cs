namespace PeaceNest.Api.Common.Auth;

public static class SupabaseJwtIssuer
{
    public static string FromProjectUrl(string projectUrl)
    {
        if (!Uri.TryCreate(projectUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("Authentication:Supabase:ProjectUrl must be an absolute Supabase project URL.");
        }

        var normalized = uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        return $"{normalized}/auth/v1";
    }
}

namespace PeaceNest.Api.Common.Auth;

public sealed class SupabaseAuthOptions
{
    public const string SectionName = "Authentication:Supabase";

    public string? ProjectUrl { get; set; }

    public string Audience { get; set; } = "authenticated";

    public string? TestingSigningKey { get; set; }
}

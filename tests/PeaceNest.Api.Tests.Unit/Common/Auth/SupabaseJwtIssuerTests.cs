using PeaceNest.Api.Common.Auth;

namespace PeaceNest.Api.Tests.Unit.Common.Auth;

public sealed class SupabaseJwtIssuerTests
{
    [Fact]
    public void FromProjectUrl_NormalizesSupabaseIssuer()
    {
        var issuer = SupabaseJwtIssuer.FromProjectUrl("https://abc123.supabase.co/");

        Assert.Equal("https://abc123.supabase.co/auth/v1", issuer);
    }

    [Fact]
    public void FromProjectUrl_RejectsRelativeUrl()
    {
        Assert.Throws<InvalidOperationException>(() => SupabaseJwtIssuer.FromProjectUrl("abc123"));
    }
}

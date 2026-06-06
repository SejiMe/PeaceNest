using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Tests.Unit.Common.RateLimiting;

public sealed class RateLimitPartitionKeyResolverTests
{
    [Fact]
    public void Resolve_UsesAuthenticatedUserSubject_WhenAvailable()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("sub", "user-123")],
                "Testing"))
        };
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");

        var key = RateLimitPartitionKeyResolver.Resolve(httpContext);

        Assert.Equal("user:user-123", key);
    }

    [Fact]
    public void Resolve_UsesIpAddress_WhenUserSubjectIsMissing()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");

        var key = RateLimitPartitionKeyResolver.Resolve(httpContext);

        Assert.Equal("ip:203.0.113.10", key);
    }

    [Fact]
    public void Resolve_UsesUnknownIp_WhenIpAddressIsMissing()
    {
        var key = RateLimitPartitionKeyResolver.Resolve(new DefaultHttpContext());

        Assert.Equal("ip:unknown", key);
    }
}

using System.Security.Claims;

namespace PeaceNest.Api.Common.RateLimiting;

public static class RateLimitPartitionKeyResolver
{
    public static string Resolve(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirstValue("sub")
            ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            return $"user:{userId}";
        }

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        return string.IsNullOrWhiteSpace(ipAddress)
            ? "ip:unknown"
            : $"ip:{ipAddress}";
    }
}

using System.Net;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Tests.Integration.Support;

namespace PeaceNest.Api.Tests.Integration.Common.RateLimiting;

public sealed class RateLimitProblemDetailsTests
{
    [Fact]
    public async Task GlobalRateLimit_ReturnsProblemDetailsWithRetryAfter()
    {
        var factory = new TestingApiFactory();
        using var client = factory.CreateClient();

        using var first = await client.GetAsync("/testing/rate-limit");
        using var second = await client.GetAsync("/testing/rate-limit");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, second.StatusCode);
        Assert.True(second.Headers.Contains("Retry-After"));
        await ProblemDetailsAssert.HasProblemDetailsAsync(
            second,
            429,
            ErrorCodes.RateLimitExceeded);
    }
}

using PeaceNest.Api.Features.System.GetHealth;

namespace PeaceNest.Api.Tests.Unit.Features.System.GetHealth;

public sealed class HealthResponseTests
{
    [Fact]
    public void Healthy_CreatesSafeHealthPayload()
    {
        var checkedAt = new DateTimeOffset(2026, 6, 6, 0, 0, 0, TimeSpan.Zero);

        var response = Response.Healthy(checkedAt);

        Assert.Equal(Response.HealthyStatus, response.Status);
        Assert.Equal(Response.ServiceName, response.Service);
        Assert.Equal(checkedAt, response.CheckedAtUtc);
    }
}

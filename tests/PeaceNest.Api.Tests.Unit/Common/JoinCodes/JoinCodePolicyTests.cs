using Microsoft.Extensions.Options;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.JoinCodes;

namespace PeaceNest.Api.Tests.Unit.Common.JoinCodes;

public sealed class JoinCodePolicyTests
{
    [Fact]
    public void Policy_UsesConfiguredAuthoritativeLifetimesAndCapacity()
    {
        var policy = new JoinCodePolicy(Options.Create(new JoinCodePolicyOptions
        {
            LifetimeMinutes = 15,
            RequestLifetimeDays = 7,
            MaxRequestsPerCode = 10
        }));
        var now = new DateTimeOffset(2026, 6, 23, 12, 0, 0, TimeSpan.Zero);
        var code = new FamilyJoinCode { RequestCount = 9, MaxRequests = 10 };

        Assert.Equal(now.AddMinutes(15), policy.GetCodeExpiry(now));
        Assert.Equal(now.AddDays(7), policy.GetRequestExpiry(now));
        Assert.True(policy.HasCapacity(code));

        code.RequestCount = 10;
        Assert.False(policy.HasCapacity(code));
    }
}

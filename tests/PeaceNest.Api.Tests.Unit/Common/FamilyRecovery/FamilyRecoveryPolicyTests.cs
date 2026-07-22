using Microsoft.Extensions.Options;
using PeaceNest.Api.Common.FamilyRecovery;

namespace PeaceNest.Api.Tests.Unit.Common.FamilyRecovery;

public sealed class FamilyRecoveryPolicyTests
{
    [Fact]
    public void Policy_UsesConfiguredRecoveryAndClaimWindows()
    {
        var policy = new FamilyRecoveryPolicy(Options.Create(new FamilyRecoveryOptions
        {
            LifetimeDays = 30,
            SweepIntervalMinutes = 60,
            ClaimLeaseMinutes = 10,
            BatchSize = 20
        }));
        var now = new DateTimeOffset(2026, 6, 24, 1, 0, 0, TimeSpan.Zero);

        Assert.Equal(now.AddDays(30), policy.GetRecoveryExpiry(now));
        Assert.Equal(now.AddMinutes(-10), policy.GetStaleClaimBoundary(now));
        Assert.False(policy.IsExpired(now.AddTicks(1), now));
        Assert.True(policy.IsExpired(now, now));
        Assert.Equal(20, policy.BatchSize);
    }
}

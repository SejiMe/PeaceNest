using Microsoft.Extensions.Options;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.JoinCodes;

public sealed class JoinCodePolicy
{
    private readonly JoinCodePolicyOptions _options;

    public JoinCodePolicy(IOptions<JoinCodePolicyOptions> options)
    {
        _options = options.Value;
    }

    public int MaxRequestsPerCode => _options.MaxRequestsPerCode;

    public DateTimeOffset GetCodeExpiry(DateTimeOffset createdAt) =>
        createdAt.AddMinutes(_options.LifetimeMinutes);

    public DateTimeOffset GetRequestExpiry(DateTimeOffset createdAt) =>
        createdAt.AddDays(_options.RequestLifetimeDays);

    public bool IsExpired(DateTimeOffset expiresAt, DateTimeOffset now) => expiresAt <= now;

    public bool HasCapacity(FamilyJoinCode code) => code.RequestCount < code.MaxRequests;
}

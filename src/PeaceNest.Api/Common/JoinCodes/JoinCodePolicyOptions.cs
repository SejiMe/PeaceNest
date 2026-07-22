namespace PeaceNest.Api.Common.JoinCodes;

public sealed class JoinCodePolicyOptions
{
    public const string SectionName = "JoinCodes";

    public int LifetimeMinutes { get; set; } = 15;

    public int RequestLifetimeDays { get; set; } = 7;

    public int MaxRequestsPerCode { get; set; } = 10;
}

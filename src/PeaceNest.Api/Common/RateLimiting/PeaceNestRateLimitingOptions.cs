namespace PeaceNest.Api.Common.RateLimiting;

public sealed class PeaceNestRateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public RateLimitRuleOptions Global { get; set; } = new()
    {
        PermitLimit = 300,
        WindowSeconds = 60
    };

    public RateLimitRuleOptions Auth { get; set; } = new()
    {
        PermitLimit = 20,
        WindowSeconds = 60
    };

    public RateLimitRuleOptions Write { get; set; } = new()
    {
        PermitLimit = 60,
        WindowSeconds = 60
    };

    public RateLimitRuleOptions Invite { get; set; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 60
    };

    public RateLimitRuleOptions RecapGeneration { get; set; } = new()
    {
        PermitLimit = 5,
        WindowSeconds = 60
    };
}

public sealed class RateLimitRuleOptions
{
    public int PermitLimit { get; set; }

    public int WindowSeconds { get; set; }
}

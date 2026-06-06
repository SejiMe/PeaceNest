namespace PeaceNest.Api.Common.RateLimiting;

public static class RateLimitPolicyNames
{
    public const string Auth = "auth";
    public const string Write = "write";
    public const string Invite = "invite";
    public const string RecapGeneration = "recap-generation";
    public const string TestingTight = "testing-tight";
}

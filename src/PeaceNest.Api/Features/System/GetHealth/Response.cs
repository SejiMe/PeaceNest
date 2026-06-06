namespace PeaceNest.Api.Features.System.GetHealth;

public sealed record Response(
    string Status,
    string Service,
    DateTimeOffset CheckedAtUtc)
{
    public const string HealthyStatus = "Healthy";
    public const string ServiceName = "PeaceNest.Api";

    public static Response Healthy(DateTimeOffset checkedAtUtc) =>
        new(HealthyStatus, ServiceName, checkedAtUtc);
}

namespace PeaceNest.Api.Common.FamilyRecovery;

public sealed class FamilyRecoveryOptions
{
    public const string SectionName = "FamilyRecovery";

    public int LifetimeDays { get; set; } = 30;

    public int SweepIntervalMinutes { get; set; } = 60;

    public int ClaimLeaseMinutes { get; set; } = 10;

    public int BatchSize { get; set; } = 20;

    public bool WorkerEnabled { get; set; } = true;
}

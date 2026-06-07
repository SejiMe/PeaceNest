namespace PeaceNest.Api.Common.Database.Entities;

public sealed class WantNeedDetails
{
    public Guid PlanId { get; set; }

    public FamilyPlan Plan { get; set; } = null!;

    public WantNeedKind Kind { get; set; }

    public decimal? EstimatedCostAmount { get; set; }

    public string? EstimatedCostCurrency { get; set; }

    public ScoreLevel UrgencyLevel { get; set; } = ScoreLevel.Medium;

    public ScoreLevel ImportanceLevel { get; set; } = ScoreLevel.Medium;

    public ScoreLevel EmotionalValueLevel { get; set; } = ScoreLevel.Medium;

    public DateOnly? DesiredByDate { get; set; }

    public string? FundingNotes { get; set; }
}

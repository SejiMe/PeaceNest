namespace PeaceNest.Api.Common.Database.Entities;

public sealed class MilestoneDetails
{
    public Guid PlanId { get; set; }

    public FamilyPlan Plan { get; set; } = null!;

    public string? MilestoneType { get; set; }

    public string? CelebrationNotes { get; set; }

    public string? ReflectionPrompt { get; set; }

    public bool IncludeInRecap { get; set; } = true;
}

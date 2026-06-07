using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.WantsAndNeeds;

public static class WantOrNeedPriorityScore
{
    public static decimal Calculate(
        WantNeedKind kind,
        ScoreLevel urgency,
        ScoreLevel importance,
        ScoreLevel emotionalValue,
        decimal? estimatedCostAmount)
    {
        var score =
            ToWeight(urgency) * 3m +
            ToWeight(importance) * 3m +
            ToWeight(emotionalValue) * 2m +
            KindWeight(kind) +
            CostSensitivityWeight(estimatedCostAmount);

        return decimal.Round(score, 2, MidpointRounding.AwayFromZero);
    }

    private static int ToWeight(ScoreLevel level) =>
        level switch
        {
            ScoreLevel.Low => 1,
            ScoreLevel.Medium => 2,
            ScoreLevel.High => 3,
            _ => 2
        };

    private static decimal KindWeight(WantNeedKind kind) =>
        kind == WantNeedKind.Need ? 2m : 0.75m;

    private static decimal CostSensitivityWeight(decimal? estimatedCostAmount)
    {
        if (estimatedCostAmount is null)
        {
            return 0m;
        }

        return estimatedCostAmount.Value switch
        {
            < 0m => 0m,
            <= 100m => 0.5m,
            <= 1000m => 1m,
            _ => 1.5m
        };
    }
}

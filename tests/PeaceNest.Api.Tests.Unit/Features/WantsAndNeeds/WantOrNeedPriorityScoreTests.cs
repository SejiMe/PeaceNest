using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Features.WantsAndNeeds;

namespace PeaceNest.Api.Tests.Unit.Features.WantsAndNeeds;

public sealed class WantOrNeedPriorityScoreTests
{
    [Fact]
    public void Calculate_GivesHigherScoreToHighUrgencyNeedThanLowWant()
    {
        var highNeed = WantOrNeedPriorityScore.Calculate(
            WantNeedKind.Need,
            ScoreLevel.High,
            ScoreLevel.High,
            ScoreLevel.Medium,
            1200m);
        var lowWant = WantOrNeedPriorityScore.Calculate(
            WantNeedKind.Want,
            ScoreLevel.Low,
            ScoreLevel.Low,
            ScoreLevel.Low,
            50m);

        Assert.True(highNeed > lowWant);
    }

    [Fact]
    public void Calculate_IsDeterministicForSameInputs()
    {
        var first = WantOrNeedPriorityScore.Calculate(
            WantNeedKind.Want,
            ScoreLevel.Medium,
            ScoreLevel.High,
            ScoreLevel.High,
            800m);
        var second = WantOrNeedPriorityScore.Calculate(
            WantNeedKind.Want,
            ScoreLevel.Medium,
            ScoreLevel.High,
            ScoreLevel.High,
            800m);

        Assert.Equal(first, second);
        Assert.Equal(22.75m, first);
    }

    [Fact]
    public void Calculate_TreatsMissingCostAsNeutral()
    {
        var score = WantOrNeedPriorityScore.Calculate(
            WantNeedKind.Need,
            ScoreLevel.Medium,
            ScoreLevel.Medium,
            ScoreLevel.Medium,
            null);

        Assert.Equal(18m, score);
    }
}

using System.Text.Json;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Recaps;

public static class MonthlyRecapResponseProjection
{
    public static MonthlyRecapResponse FromRecap(Recap recap)
    {
        var stats = JsonSerializer.Deserialize<MonthlyRecapStatsResponse>(recap.Stats.RootElement.GetRawText()) ??
            new MonthlyRecapStatsResponse(0, 0, 0, 0, 0, 0, 0, 0);

        var items = recap.Items
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Title)
            .Select(item => new RecapItemResponse(
                item.Id,
                item.PlanId,
                item.MemoryId,
                item.ItemType,
                item.Title,
                item.Description,
                item.SortOrder,
                item.CreatedAt))
            .ToArray();

        return new MonthlyRecapResponse(
            recap.Id,
            recap.FamilyId,
            recap.PeriodType,
            recap.PeriodStart,
            recap.PeriodEnd,
            recap.Title,
            recap.Summary,
            stats,
            recap.GeneratedByUserId,
            recap.PublishedAt,
            recap.CreatedAt,
            recap.UpdatedAt,
            items);
    }
}

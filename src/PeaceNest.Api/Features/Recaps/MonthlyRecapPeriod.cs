using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.Recaps;

public sealed record MonthlyRecapPeriod(int Year, int Month, DateOnly Start, DateOnly End)
{
    public static MonthlyRecapPeriod FromRoute(int year, int month)
    {
        var failures = new List<ValidationFailure>();

        if (year is < 2000 or > 2100)
        {
            failures.Add(new ValidationFailure("year", "Year must be between 2000 and 2100."));
        }

        if (month is < 1 or > 12)
        {
            failures.Add(new ValidationFailure("month", "Month must be between 1 and 12."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationAppException("Monthly recap period is invalid.", failures);
        }

        var start = new DateOnly(year, month, 1);
        return new MonthlyRecapPeriod(year, month, start, start.AddMonths(1).AddDays(-1));
    }

    public DateTimeOffset StartAtUtc =>
        new(Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero);

    public DateTimeOffset ExclusiveEndAtUtc =>
        new(End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero);

    public string MonthName =>
        Start.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy", global::System.Globalization.CultureInfo.InvariantCulture);
}

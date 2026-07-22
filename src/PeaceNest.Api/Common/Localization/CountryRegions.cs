using System.Globalization;

namespace PeaceNest.Api.Common.Localization;

public static class CountryRegions
{
    private static readonly HashSet<string> Codes = CultureInfo
        .GetCultures(CultureTypes.SpecificCultures)
        .Select(culture => new RegionInfo(culture.Name).TwoLetterISORegionName)
        .Concat(["AQ", "BV", "EH", "GS", "HM", "TF"])
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsSupported(string? countryCode) =>
        !string.IsNullOrWhiteSpace(countryCode) && Codes.Contains(countryCode.Trim());

    public static string Normalize(string countryCode) =>
        countryCode.Trim().ToUpperInvariant();
}

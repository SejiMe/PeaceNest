namespace PeaceNest.Api.Common.Localization;

public static class FamilyCurrencies
{
    public const string Default = "PHP";

    private static readonly HashSet<string> Supported =
        ["PHP", "SGD", "USD"];

    public static bool IsSupported(string? currency) =>
        !string.IsNullOrWhiteSpace(currency) && Supported.Contains(currency.Trim().ToUpperInvariant());

    public static string Normalize(string currency) =>
        currency.Trim().ToUpperInvariant();

    public static string SuggestForCountry(string? countryCode) =>
        countryCode?.Trim().ToUpperInvariant() switch
        {
            "SG" => "SGD",
            "US" => "USD",
            _ => Default
        };
}

using PeaceNest.Api.Common.Localization;

namespace PeaceNest.Api.Tests.Unit.Common.Localization;

public sealed class FamilyCurrenciesTests
{
    [Theory]
    [InlineData("PH", "PHP")]
    [InlineData("SG", "SGD")]
    [InlineData("US", "USD")]
    [InlineData("JP", "PHP")]
    public void SuggestForCountry_UsesOnlyMvpCurrencies(string countryCode, string expected) =>
        Assert.Equal(expected, FamilyCurrencies.SuggestForCountry(countryCode));

    [Theory]
    [InlineData("PHP", true)]
    [InlineData("sgd", true)]
    [InlineData("JPY", false)]
    public void IsSupported_RestrictsMvpCurrencySet(string currency, bool expected) =>
        Assert.Equal(expected, FamilyCurrencies.IsSupported(currency));
}

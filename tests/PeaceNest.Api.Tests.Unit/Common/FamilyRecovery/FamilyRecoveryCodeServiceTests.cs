using PeaceNest.Api.Common.FamilyRecovery;

namespace PeaceNest.Api.Tests.Unit.Common.FamilyRecovery;

public sealed class FamilyRecoveryCodeServiceTests
{
    private readonly FamilyRecoveryCodeService _service = new();

    [Fact]
    public void GenerateCode_ReturnsHumanFriendlyTwentyCharacterCode()
    {
        var code = _service.GenerateCode();

        Assert.Matches(
            "^[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{5}(-[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{5}){3}$",
            code);
        Assert.True(_service.IsValid(code));
    }

    [Fact]
    public void Hash_NormalizesCaseWhitespaceAndSeparators()
    {
        var expected = _service.Hash("ABCDE-FGHIJ-KLMNP-QRSTU");

        Assert.Equal(expected, _service.Hash(" abcde fghij klmnp qrstu "));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABCDE-FGHIJ")]
    [InlineData("ABCDE-FGHIJ-KLMNP-QRST0")]
    [InlineData("ABCDE-FGHIJ-KLMNO-QRSTU")]
    public void IsValid_RejectsMalformedOrAmbiguousCodes(string code)
    {
        Assert.False(_service.IsValid(code));
    }
}

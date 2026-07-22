using PeaceNest.Api.Common.JoinCodes;

namespace PeaceNest.Api.Tests.Unit.Common.JoinCodes;

public sealed class FamilyJoinCodeServiceTests
{
    private readonly FamilyJoinCodeService _service = new();

    [Fact]
    public void GenerateCode_ReturnsHumanFriendlyTenCharacterCode()
    {
        var code = _service.GenerateCode();

        Assert.Matches("^[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{5}-[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{5}$", code);
        Assert.True(_service.IsValid(code));
    }

    [Fact]
    public void Hash_NormalizesCaseWhitespaceAndSeparator()
    {
        var expected = _service.Hash("ABCDE-FGHIJ");

        Assert.Equal(expected, _service.Hash(" abcde fghij "));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABCDE")]
    [InlineData("ABCDE-0GHIJ")]
    [InlineData("ABCDE-OGHIJ")]
    public void IsValid_RejectsMalformedOrAmbiguousCodes(string code)
    {
        Assert.False(_service.IsValid(code));
    }
}

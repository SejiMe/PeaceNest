using PeaceNest.Api.Features.FamilyJoinRequests;

namespace PeaceNest.Api.Tests.Unit.Features.FamilyJoinRequests;

public sealed class FamilyJoinRequestResponseProjectionTests
{
    [Theory]
    [InlineData("mia@gmail.com", "m***@gmail.com")]
    [InlineData("a@example.test", "a***@example.test")]
    [InlineData("invalid", "***")]
    public void MaskEmail_RevealsOnlyMinimalVerifiedIdentity(string email, string expected)
    {
        Assert.Equal(expected, FamilyJoinRequestResponseProjection.MaskEmail(email));
    }
}

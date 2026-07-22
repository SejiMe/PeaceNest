using PeaceNest.Api.Common.Security;

namespace PeaceNest.Api.Tests.Unit.Common.Security;

public sealed class InvitationTokenServiceTests
{
    [Fact]
    public void GenerateToken_ReturnsUrlSafeHighEntropyToken()
    {
        var service = new InvitationTokenService();

        var token = service.GenerateToken();

        Assert.NotEmpty(token);
        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
    }

    [Fact]
    public void HashToken_DoesNotReturnTheRawToken()
    {
        var service = new InvitationTokenService();
        var token = service.GenerateToken();

        var hash = service.HashToken(token);

        Assert.NotEqual(token, hash);
        Assert.Equal(64, hash.Length);
        Assert.Equal(hash, service.HashToken(token));
    }

    [Fact]
    public void GenerateCode_ReturnsNormalizedHumanFriendlySingleUseCode()
    {
        var service = new InvitationTokenService();

        var code = service.GenerateCode();

        Assert.Equal(11, code.Length);
        Assert.Equal('-', code[5]);
        Assert.Equal(10, service.NormalizeCode(code).Length);
        Assert.Equal(service.HashCode(code), service.HashCode(code.ToLowerInvariant().Replace("-", " ")));
    }
}

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
}

using Microsoft.Extensions.DependencyInjection;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Tests.Integration.Support;

namespace PeaceNest.Api.Tests.Integration.Common.Database;

public sealed class DatabaseRegistrationTests : IClassFixture<TestingApiFactory>
{
    private readonly TestingApiFactory _factory;

    public DatabaseRegistrationTests(TestingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Services_CanResolvePeaceNestDbContext()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();

        Assert.NotNull(dbContext);
    }
}

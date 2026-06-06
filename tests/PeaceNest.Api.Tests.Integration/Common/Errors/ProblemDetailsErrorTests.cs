using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Tests.Integration.Support;

namespace PeaceNest.Api.Tests.Integration.Common.Errors;

public sealed class ProblemDetailsErrorTests : IClassFixture<TestingApiFactory>
{
    private readonly TestingApiFactory _factory;

    public ProblemDetailsErrorTests(TestingApiFactory factory)
    {
        _factory = factory;
    }

    public static TheoryData<string, int, string> KnownErrors =>
        new()
        {
            { "validation", 400, ErrorCodes.ValidationFailed },
            { "authentication", 401, ErrorCodes.AuthenticationRequired },
            { "authorization", 403, ErrorCodes.AuthorizationDenied },
            { "not-found", 404, ErrorCodes.ResourceNotFound },
            { "conflict", 409, ErrorCodes.ResourceConflict },
            { "domain", 422, ErrorCodes.DomainRuleViolated },
            { "external-provider", 502, ErrorCodes.ExternalProviderUnavailable }
        };

    [Theory]
    [MemberData(nameof(KnownErrors))]
    public async Task TestingErrorEndpoint_ReturnsProblemDetails_ForKnownErrors(
        string kind,
        int expectedStatusCode,
        string expectedErrorCode)
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync($"/testing/errors/{kind}");

        await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            expectedStatusCode,
            expectedErrorCode);
    }

    [Fact]
    public async Task TestingErrorEndpoint_HidesUnexpectedDetailsOutsideDevelopment()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/testing/errors/unexpected");
        var problemDetails = await ProblemDetailsAssert.HasProblemDetailsAsync(
            response,
            500,
            ErrorCodes.Unexpected);

        Assert.Contains("Unexpected diagnostic failure", problemDetails.Detail);
    }
}

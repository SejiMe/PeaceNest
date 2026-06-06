using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Tests.Unit.Common.Errors;

public sealed class ApiErrorMapperTests
{
    public static TheoryData<Exception, int, string> KnownExceptions =>
        new()
        {
            { new ValidationAppException("Validation failed.", [new("name", "Name is required.")]), 400, ErrorCodes.ValidationFailed },
            { new AuthenticationAppException("Authentication required."), 401, ErrorCodes.AuthenticationRequired },
            { new AuthorizationAppException("Access denied."), 403, ErrorCodes.AuthorizationDenied },
            { new NotFoundAppException("Missing."), 404, ErrorCodes.ResourceNotFound },
            { new ConflictAppException("Conflict."), 409, ErrorCodes.ResourceConflict },
            { new DomainRuleAppException("Rule broken."), 422, ErrorCodes.DomainRuleViolated },
            { new ExternalProviderAppException("Supabase unavailable."), 502, ErrorCodes.ExternalProviderUnavailable }
        };

    [Theory]
    [MemberData(nameof(KnownExceptions))]
    public void Map_ReturnsExpectedDescriptor_ForKnownException(
        Exception exception,
        int expectedStatusCode,
        string expectedErrorCode)
    {
        var descriptor = ApiErrorMapper.Map(exception);

        Assert.Equal(expectedStatusCode, descriptor.StatusCode);
        Assert.Equal(expectedErrorCode, descriptor.ErrorCode);
    }

    [Fact]
    public void Map_HidesUnexpectedDetails_ByDefault()
    {
        var descriptor = ApiErrorMapper.Map(new InvalidOperationException("Sensitive internal detail."));

        Assert.Equal(500, descriptor.StatusCode);
        Assert.Equal(ErrorCodes.Unexpected, descriptor.ErrorCode);
        Assert.DoesNotContain("Sensitive", descriptor.Message);
    }

    [Fact]
    public void Map_IncludesValidationDetails_ForValidationException()
    {
        var descriptor = ApiErrorMapper.Map(new ValidationAppException(
            "Validation failed.",
            [
                new("title", "Title is required."),
                new("title", "Title is too long.")
            ]));

        Assert.NotNull(descriptor.ValidationDetails);
        Assert.Equal(
            ["Title is required.", "Title is too long."],
            descriptor.ValidationDetails["title"]);
    }
}

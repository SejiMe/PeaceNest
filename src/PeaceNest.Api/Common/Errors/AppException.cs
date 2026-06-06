namespace PeaceNest.Api.Common.Errors;

public abstract class AppException : Exception
{
    protected AppException(string errorCode, string message, int statusCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public string ErrorCode { get; }

    public int StatusCode { get; }

    public virtual IReadOnlyDictionary<string, string[]>? ValidationDetails => null;
}

public sealed class ValidationAppException : AppException
{
    public ValidationAppException(string message, IReadOnlyCollection<ValidationFailure> failures)
        : base(ErrorCodes.ValidationFailed, message, StatusCodes.Status400BadRequest)
    {
        ValidationDetails = failures
            .GroupBy(failure => failure.Field)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.Message).ToArray());
    }

    public override IReadOnlyDictionary<string, string[]> ValidationDetails { get; }
}

public sealed class AuthenticationAppException : AppException
{
    public AuthenticationAppException(string message)
        : base(ErrorCodes.AuthenticationRequired, message, StatusCodes.Status401Unauthorized)
    {
    }
}

public sealed class AuthorizationAppException : AppException
{
    public AuthorizationAppException(string message)
        : base(ErrorCodes.AuthorizationDenied, message, StatusCodes.Status403Forbidden)
    {
    }
}

public sealed class NotFoundAppException : AppException
{
    public NotFoundAppException(string message)
        : base(ErrorCodes.ResourceNotFound, message, StatusCodes.Status404NotFound)
    {
    }
}

public sealed class ConflictAppException : AppException
{
    public ConflictAppException(string message)
        : base(ErrorCodes.ResourceConflict, message, StatusCodes.Status409Conflict)
    {
    }
}

public sealed class DomainRuleAppException : AppException
{
    public DomainRuleAppException(string message)
        : base(ErrorCodes.DomainRuleViolated, message, StatusCodes.Status422UnprocessableEntity)
    {
    }
}

public sealed class ExternalProviderAppException : AppException
{
    public ExternalProviderAppException(string message)
        : base(ErrorCodes.ExternalProviderUnavailable, message, StatusCodes.Status502BadGateway)
    {
    }
}

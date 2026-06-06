namespace PeaceNest.Api.Common.Errors;

public sealed record ApiErrorDescriptor(
    int StatusCode,
    string ErrorCode,
    string Title,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationDetails = null)
{
    public static ApiErrorDescriptor Authentication(string errorCode, string message) =>
        new(StatusCodes.Status401Unauthorized, errorCode, "Authentication required", message);

    public static ApiErrorDescriptor Authorization(string errorCode, string message) =>
        new(StatusCodes.Status403Forbidden, errorCode, "Access denied", message);

    public static ApiErrorDescriptor RateLimit(string message) =>
        new(StatusCodes.Status429TooManyRequests, ErrorCodes.RateLimitExceeded, "Too many requests", message);
}

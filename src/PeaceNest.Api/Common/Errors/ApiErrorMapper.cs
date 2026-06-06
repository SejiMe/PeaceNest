namespace PeaceNest.Api.Common.Errors;

public static class ApiErrorMapper
{
    public static ApiErrorDescriptor Map(Exception exception, bool includeUnexpectedDetails = false)
    {
        if (exception is AppException appException)
        {
            return new ApiErrorDescriptor(
                appException.StatusCode,
                appException.ErrorCode,
                GetTitle(appException.StatusCode),
                appException.Message,
                appException.ValidationDetails);
        }

        return new ApiErrorDescriptor(
            StatusCodes.Status500InternalServerError,
            ErrorCodes.Unexpected,
            "Unexpected server error",
            includeUnexpectedDetails
                ? exception.Message
                : "Something went wrong while PeaceNest handled the request.");
    }

    private static string GetTitle(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "Validation failed",
            StatusCodes.Status401Unauthorized => "Authentication required",
            StatusCodes.Status403Forbidden => "Access denied",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status422UnprocessableEntity => "Domain rule violated",
            StatusCodes.Status502BadGateway => "External provider unavailable",
            _ => "Request failed"
        };
}

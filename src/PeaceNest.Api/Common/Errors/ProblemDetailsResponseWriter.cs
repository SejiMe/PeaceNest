using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PeaceNest.Api.Common.Errors;

public static class ProblemDetailsResponseWriter
{
    public static async Task WriteAsync(
        HttpContext httpContext,
        ApiErrorDescriptor error,
        IProblemDetailsService problemDetailsService,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = error.StatusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = error.ErrorCode;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (error.ValidationDetails is not null)
        {
            problemDetails.Extensions["validationDetails"] = error.ValidationDetails;
        }

        var wasWritten = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });

        if (!wasWritten)
        {
            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(problemDetails),
                cancellationToken);
        }
    }
}

using Microsoft.AspNetCore.Diagnostics;

namespace PeaceNest.Api.Common.Errors;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ApiExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public ApiExceptionHandler(
        IHostEnvironment environment,
        ILogger<ApiExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _environment = environment;
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is AppException)
        {
            _logger.LogInformation(
                exception,
                "Handled API exception {TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogError(
                exception,
                "Unhandled API exception {TraceId}",
                httpContext.TraceIdentifier);
        }

        var error = ApiErrorMapper.Map(
            exception,
            _environment.IsDevelopment() || _environment.IsEnvironment("Testing"));

        await ProblemDetailsResponseWriter.WriteAsync(
            httpContext,
            error,
            _problemDetailsService,
            cancellationToken);

        return true;
    }
}

using FastEndpoints;

namespace PeaceNest.Api.Features.System.GetHealth;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
        Description(builder => builder.WithTags("System"));
        Summary(summary =>
        {
            summary.Summary = "Check API health.";
            summary.Description = "Returns a lightweight health response without sensitive dependency details.";
            summary.Responses[200] = "The API is healthy.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(Response.Healthy(DateTimeOffset.UtcNow), ct);
    }
}

using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PeaceNest.Api.Common.Auth;

namespace PeaceNest.Api.Features.Auth.GetProtectedSmoke;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/auth/protected-smoke");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Auth"));
        Summary(summary =>
        {
            summary.Summary = "Verify protected API access.";
            summary.Description = "Returns the authenticated Supabase user claims needed by the backend safety foundation.";
            summary.Responses[200] = "The access token was accepted.";
            summary.Responses[401] = "A valid Supabase access token is required.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var subject = User.FindFirst(AuthClaimTypes.Subject)?.Value;
        var email = User.FindFirst(AuthClaimTypes.Email)?.Value;
        var role = User.FindFirst(AuthClaimTypes.Role)?.Value;

        await Send.OkAsync(new Response(subject, email, role), ct);
    }
}

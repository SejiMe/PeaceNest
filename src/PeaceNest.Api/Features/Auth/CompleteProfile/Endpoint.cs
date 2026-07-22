using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.Localization;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.Auth.CompleteProfile;

public sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly PeaceNestDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public Endpoint(CurrentUserService currentUserService, PeaceNestDbContext dbContext, TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Put("/auth/profile");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Auth"));
        Summary(summary =>
        {
            summary.Summary = "Complete the PeaceNest profile.";
            summary.Description = "Confirms the authenticated user's global display name and country or region.";
            summary.Responses[200] = "The profile was completed.";
            summary.Responses[400] = "The profile request was invalid.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        ValidateRequest(request);

        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        var completedAt = user.OnboardingCompletedAt ?? _timeProvider.GetUtcNow();

        user.DisplayName = request.DisplayName.Trim();
        user.CountryCode = CountryRegions.Normalize(request.CountryCode);
        user.OnboardingCompletedAt = completedAt;
        await _dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(
            new Response(user.Id, user.Email, user.DisplayName, user.CountryCode, completedAt),
            ct);
    }

    private static void ValidateRequest(Request request)
    {
        var failures = new List<ValidationFailure>();
        var displayName = request.DisplayName?.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            failures.Add(new ValidationFailure("displayName", "Display name is required."));
        }
        else if (displayName.Length > 200)
        {
            failures.Add(new ValidationFailure("displayName", "Display name must be 200 characters or fewer."));
        }

        if (!CountryRegions.IsSupported(request.CountryCode))
        {
            failures.Add(new ValidationFailure("countryCode", "Select a valid country or region."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationAppException("PeaceNest profile request is invalid.", failures);
        }
    }
}

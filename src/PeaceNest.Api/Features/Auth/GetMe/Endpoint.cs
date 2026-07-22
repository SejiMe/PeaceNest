using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Auth.GetMe;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly PeaceNestDbContext _dbContext;

    public Endpoint(CurrentUserService currentUserService, PeaceNestDbContext dbContext)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/auth/me");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Auth"));
        Summary(summary =>
        {
            summary.Summary = "Get the current PeaceNest user.";
            summary.Description = "Mirrors the authenticated Supabase Google user on first request and returns family memberships.";
            summary.Responses[200] = "The authenticated user and family memberships were returned.";
            summary.Responses[401] = "A valid Supabase Google access token is required.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);

        var memberships = await _dbContext.FamilyMembers
            .AsNoTracking()
            .Include(member => member.Family)
            .Where(member => member.UserId == user.Id && member.Status == FamilyMemberStatus.Active)
            .OrderBy(member => member.Family.Name)
            .Select(member => new FamilyMembershipResponse(
                member.FamilyId,
                member.Family.Name,
                member.Family.PreferredCurrency,
                member.Role,
                member.Status))
            .ToListAsync(ct);

        await Send.OkAsync(
            new Response(
                new CurrentUserResponse(
                    user.Id,
                    user.SupabaseUserId,
                    user.Email,
                    user.DisplayName,
                    user.CountryCode,
                    user.OnboardingCompletedAt,
                    user.AvatarUrl,
                    user.Timezone,
                    user.LastSeenAt),
                memberships),
            ct);
    }
}

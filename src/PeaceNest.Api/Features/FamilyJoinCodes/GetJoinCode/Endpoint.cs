using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.FamilyJoinCodes.GetJoinCode;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly FamilyMembershipAuthorizer _authorizer;
    private readonly PeaceNestDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        FamilyMembershipAuthorizer authorizer,
        PeaceNestDbContext dbContext,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _authorizer = authorizer;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Get("/families/{familyId:guid}/join-code");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Family Join Codes"));
        Summary(summary =>
        {
            summary.Summary = "Inspect the active family join code.";
            summary.Description = "Returns active-code metadata without revealing its plaintext value.";
            summary.Responses[200] = "The join-code status was returned.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        await _authorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanInviteFamilyMembers,
            "Only family owners and parent/admins can view join-code status.",
            ct);

        var now = _timeProvider.GetUtcNow();
        var code = await _dbContext.FamilyJoinCodes
            .AsNoTracking()
            .Where(candidate => candidate.FamilyId == familyId && candidate.Status == FamilyJoinCodeStatus.Active)
            .OrderByDescending(candidate => candidate.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (code is null || code.ExpiresAt <= now)
        {
            await Send.OkAsync(new Response(false, null, null, null, null, null, null), ct);
            return;
        }

        await Send.OkAsync(
            new Response(
                true,
                code.Id,
                code.Status,
                code.RequestCount,
                code.MaxRequests,
                code.CreatedAt,
                code.ExpiresAt),
            ct);
    }
}

using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyJoinCodes.RevokeJoinCode;

public sealed class Endpoint : EndpointWithoutRequest
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
        Delete("/families/{familyId:guid}/join-code");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Invite));
        Description(builder => builder.WithTags("Family Join Codes"));
        Summary(summary => summary.Summary = "Revoke the active family join code.");
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
            "Only family owners and parent/admins can revoke join codes.",
            ct);

        var activeCodes = await _dbContext.FamilyJoinCodes
            .Where(code => code.FamilyId == familyId && code.Status == FamilyJoinCodeStatus.Active)
            .ToListAsync(ct);
        var now = _timeProvider.GetUtcNow();
        foreach (var code in activeCodes)
        {
            code.Status = code.ExpiresAt <= now
                ? FamilyJoinCodeStatus.Expired
                : FamilyJoinCodeStatus.Revoked;
            code.RevokedAt = code.ExpiresAt <= now ? null : now;
            code.RevokedByUserId = code.ExpiresAt <= now ? null : user.Id;
        }

        await _dbContext.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}

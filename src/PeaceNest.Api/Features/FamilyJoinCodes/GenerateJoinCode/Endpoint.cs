using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.JoinCodes;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyJoinCodes.GenerateJoinCode;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly FamilyMembershipAuthorizer _authorizer;
    private readonly PeaceNestDbContext _dbContext;
    private readonly FamilyJoinCodeService _codeService;
    private readonly JoinCodePolicy _policy;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        FamilyMembershipAuthorizer authorizer,
        PeaceNestDbContext dbContext,
        FamilyJoinCodeService codeService,
        JoinCodePolicy policy,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _authorizer = authorizer;
        _dbContext = dbContext;
        _codeService = codeService;
        _policy = policy;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Post("/families/{familyId:guid}/join-code");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Invite));
        Description(builder => builder.WithTags("Family Join Codes"));
        Summary(summary =>
        {
            summary.Summary = "Generate a temporary family join code.";
            summary.Description = "Rotates any active code and reveals the new plaintext code once.";
            summary.Responses[201] = "The join code was generated.";
            summary.Responses[403] = "The authenticated member cannot manage family join codes.";
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
            "Only family owners and parent/admins can manage join codes.",
            ct);

        var now = _timeProvider.GetUtcNow();
        var activeCodes = await _dbContext.FamilyJoinCodes
            .Where(code => code.FamilyId == familyId && code.Status == FamilyJoinCodeStatus.Active)
            .ToListAsync(ct);
        foreach (var activeCode in activeCodes)
        {
            activeCode.Status = activeCode.ExpiresAt <= now
                ? FamilyJoinCodeStatus.Expired
                : FamilyJoinCodeStatus.Revoked;
            activeCode.RevokedAt = activeCode.ExpiresAt <= now ? null : now;
            activeCode.RevokedByUserId = activeCode.ExpiresAt <= now ? null : user.Id;
        }

        var plaintextCode = _codeService.GenerateCode();
        var code = new FamilyJoinCode
        {
            FamilyId = familyId,
            CodeHash = _codeService.Hash(plaintextCode),
            Status = FamilyJoinCodeStatus.Active,
            RequestCount = 0,
            MaxRequests = _policy.MaxRequestsPerCode,
            ExpiresAt = _policy.GetCodeExpiry(now),
            CreatedByUserId = user.Id
        };
        _dbContext.FamilyJoinCodes.Add(code);
        await _dbContext.SaveChangesAsync(ct);

        await Send.CreatedAtAsync(
            nameof(GetJoinCode.Endpoint),
            new { familyId },
            new Response(
                code.Id,
                plaintextCode,
                code.Status,
                code.RequestCount,
                code.MaxRequests,
                code.CreatedAt,
                code.ExpiresAt),
            cancellation: ct);
    }
}

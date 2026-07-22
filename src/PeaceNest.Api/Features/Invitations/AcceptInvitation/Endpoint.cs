using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;
using PeaceNest.Api.Common.Security;

namespace PeaceNest.Api.Features.Invitations.AcceptInvitation;

public sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly PeaceNestDbContext _dbContext;
    private readonly InvitationTokenService _invitationTokenService;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        PeaceNestDbContext dbContext,
        InvitationTokenService invitationTokenService,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _invitationTokenService = invitationTokenService;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Post("/family-invitations/accept");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Invite));
        Description(builder => builder.WithTags("Invitations"));
        Summary(summary =>
        {
            summary.Summary = "Accept a family invitation.";
            summary.Description = "Accepts an email-bound invitation when the authenticated Google email matches the invitation.";
            summary.Responses[200] = "The family invitation was accepted.";
            summary.Responses[400] = "The invitation token was missing.";
            summary.Responses[403] = "The authenticated Google email does not match the invitation.";
            summary.Responses[404] = "The invitation token was not found.";
            summary.Responses[409] = "The invitation cannot be accepted because membership already exists.";
            summary.Responses[422] = "The invitation is expired or no longer pending.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        ValidateRequest(request);

        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        CurrentUserService.RequireCompletedProfile(user);
        var tokenHash = string.IsNullOrWhiteSpace(request.InvitationToken)
            ? null
            : _invitationTokenService.HashToken(request.InvitationToken.Trim());
        var codeHash = string.IsNullOrWhiteSpace(request.InvitationCode)
            ? null
            : _invitationTokenService.HashCode(request.InvitationCode);
        var now = _timeProvider.GetUtcNow();

        var invitation = await _dbContext.FamilyInvitations
            .Include(invitation => invitation.Family)
            .SingleOrDefaultAsync(
                invitation =>
                    (tokenHash != null && invitation.TokenHash == tokenHash) ||
                    (codeHash != null && invitation.InvitationCodeHash == codeHash),
                ct);

        if (invitation is null)
        {
            throw new NotFoundAppException("Family invitation was not found.");
        }

        if (invitation.Status != FamilyInvitationStatus.Pending)
        {
            throw new DomainRuleAppException("This family invitation is no longer pending.");
        }

        if (invitation.ExpiresAt <= now)
        {
            invitation.Status = FamilyInvitationStatus.Expired;
            await _dbContext.SaveChangesAsync(ct);
            throw new DomainRuleAppException("This family invitation has expired.");
        }

        if (!string.Equals(invitation.InvitedEmail, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new AuthorizationAppException("This invitation belongs to a different Google email.");
        }

        var existingMembership = await _dbContext.FamilyMembers
            .SingleOrDefaultAsync(
                member => member.FamilyId == invitation.FamilyId && member.UserId == user.Id,
                ct);

        if (existingMembership is { Status: FamilyMemberStatus.Active })
        {
            throw new ConflictAppException("You already belong to this family workspace.");
        }

        var membership = existingMembership ?? new FamilyMember
        {
            Id = Guid.NewGuid(),
            FamilyId = invitation.FamilyId,
            UserId = user.Id
        };

        membership.Role = invitation.InvitedRole;
        membership.Status = FamilyMemberStatus.Active;
        membership.JoinedAt = now;
        membership.RemovedAt = null;

        if (existingMembership is null)
        {
            _dbContext.FamilyMembers.Add(membership);
        }

        invitation.Status = FamilyInvitationStatus.Accepted;
        invitation.AcceptedByUserId = user.Id;
        invitation.AcceptedAt = now;

        await _dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(
            new Response(
                invitation.Id,
                invitation.FamilyId,
                invitation.Family.Name,
                membership.Role,
                membership.Status,
                now),
            ct);
    }

    private static void ValidateRequest(Request request)
    {
        var hasToken = !string.IsNullOrWhiteSpace(request.InvitationToken);
        var hasCode = !string.IsNullOrWhiteSpace(request.InvitationCode);

        if (hasToken == hasCode)
        {
            throw new ValidationAppException(
                "Family invitation request is invalid.",
                [new ValidationFailure("invitation", "Provide either an invitation link token or invitation code.")]);
        }
    }
}

using System.Net.Mail;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;
using PeaceNest.Api.Common.Security;

namespace PeaceNest.Api.Features.Invitations.CreateInvitation;

public sealed class Endpoint : Endpoint<Request, Response>
{
    private static readonly TimeSpan InvitationLifetime = TimeSpan.FromDays(7);

    private readonly CurrentUserService _currentUserService;
    private readonly FamilyMembershipAuthorizer _familyMembershipAuthorizer;
    private readonly PeaceNestDbContext _dbContext;
    private readonly InvitationTokenService _invitationTokenService;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        FamilyMembershipAuthorizer familyMembershipAuthorizer,
        PeaceNestDbContext dbContext,
        InvitationTokenService invitationTokenService,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _familyMembershipAuthorizer = familyMembershipAuthorizer;
        _dbContext = dbContext;
        _invitationTokenService = invitationTokenService;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Post("/families/{familyId:guid}/invitations");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Invite));
        Description(builder => builder.WithTags("Invitations"));
        Summary(summary =>
        {
            summary.Summary = "Invite a family member.";
            summary.Description = "Creates an email-bound invitation token for a family workspace.";
            summary.Responses[201] = "The family invitation was created.";
            summary.Responses[400] = "The invitation request was invalid.";
            summary.Responses[403] = "The authenticated family member cannot invite members.";
            summary.Responses[409] = "The invited email already has an active pending invitation or membership.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        ValidateRequest(request);

        var familyId = Route<Guid>("familyId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanInviteFamilyMembers,
            "Only family owners and parent/admins can invite family members.",
            ct);

        var now = _timeProvider.GetUtcNow();
        var invitedEmail = NormalizeEmail(request.InvitedEmail);

        await ExpireStaleInvitationsAsync(familyId, invitedEmail, now, ct);
        await EnsureEmailCanBeInvitedAsync(familyId, invitedEmail, now, ct);

        var token = _invitationTokenService.GenerateToken();
        var invitation = new FamilyInvitation
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            InvitedEmail = invitedEmail,
            InvitedRole = request.InvitedRole,
            TokenHash = _invitationTokenService.HashToken(token),
            Status = FamilyInvitationStatus.Pending,
            ExpiresAt = now.Add(InvitationLifetime),
            CreatedByUserId = user.Id
        };

        _dbContext.FamilyInvitations.Add(invitation);
        await _dbContext.SaveChangesAsync(ct);

        await Send.CreatedAtAsync(
            nameof(GetInvitation.Endpoint),
            new { familyId, invitationId = invitation.Id },
            new Response(
                invitation.Id,
                invitation.FamilyId,
                invitation.InvitedEmail,
                invitation.InvitedRole,
                invitation.Status,
                invitation.ExpiresAt,
                token),
            cancellation: ct);
    }

    private static void ValidateRequest(Request request)
    {
        var failures = new List<ValidationFailure>();

        if (!IsValidEmail(request.InvitedEmail))
        {
            failures.Add(new ValidationFailure("invitedEmail", "A valid invitation email is required."));
        }

        if (!Enum.IsDefined(request.InvitedRole))
        {
            failures.Add(new ValidationFailure("invitedRole", "A valid family role is required."));
        }
        else if (request.InvitedRole == FamilyMemberRole.Owner)
        {
            failures.Add(new ValidationFailure("invitedRole", "Owner cannot be assigned through an invitation."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationAppException("Family invitation request is invalid.", failures);
        }
    }

    private async Task ExpireStaleInvitationsAsync(
        Guid familyId,
        string invitedEmail,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var staleInvitations = await _dbContext.FamilyInvitations
            .Where(invitation => invitation.FamilyId == familyId &&
                invitation.InvitedEmail == invitedEmail &&
                invitation.Status == FamilyInvitationStatus.Pending &&
                invitation.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        foreach (var invitation in staleInvitations)
        {
            invitation.Status = FamilyInvitationStatus.Expired;
        }
    }

    private async Task EnsureEmailCanBeInvitedAsync(
        Guid familyId,
        string invitedEmail,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var hasActivePendingInvitation = await _dbContext.FamilyInvitations
            .AnyAsync(invitation => invitation.FamilyId == familyId &&
                invitation.InvitedEmail == invitedEmail &&
                invitation.Status == FamilyInvitationStatus.Pending &&
                invitation.ExpiresAt > now,
                cancellationToken);

        if (hasActivePendingInvitation)
        {
            throw new ConflictAppException("This email already has a pending family invitation.");
        }

        var isActiveMember = await _dbContext.FamilyMembers
            .AnyAsync(member => member.FamilyId == familyId &&
                member.Status == FamilyMemberStatus.Active &&
                member.User.Email == invitedEmail,
                cancellationToken);

        if (isActiveMember)
        {
            throw new ConflictAppException("This email already belongs to this family workspace.");
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Trim().Length > 320)
        {
            return false;
        }

        try
        {
            var address = new MailAddress(email.Trim());
            return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();
}

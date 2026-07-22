using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.Localization;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.Families.UpdatePreferredCurrency;

public sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly FamilyMembershipAuthorizer _familyMembershipAuthorizer;
    private readonly PeaceNestDbContext _dbContext;

    public Endpoint(
        CurrentUserService currentUserService,
        FamilyMembershipAuthorizer familyMembershipAuthorizer,
        PeaceNestDbContext dbContext)
    {
        _currentUserService = currentUserService;
        _familyMembershipAuthorizer = familyMembershipAuthorizer;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put("/families/{familyId:guid}/preferences/currency");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Families"));
        Summary(summary =>
        {
            summary.Summary = "Change the family workspace preferred currency.";
            summary.Description = "Changes the default for new estimates without converting existing plan amounts.";
            summary.Responses[200] = "The preferred currency was changed.";
            summary.Responses[403] = "Only owners and parent/admins can change family settings.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        if (!FamilyCurrencies.IsSupported(request.PreferredCurrency))
        {
            throw new ValidationAppException(
                "Family currency request is invalid.",
                [new ValidationFailure("preferredCurrency", "Select PHP, SGD, or USD for this family workspace.")]);
        }

        var familyId = Route<Guid>("familyId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanManageFamilySettings,
            "Only family owners and parent/admins can change family settings.",
            ct);

        var family = await _dbContext.Families.SingleOrDefaultAsync(candidate => candidate.Id == familyId, ct);
        if (family is null)
        {
            throw new NotFoundAppException("Family workspace was not found.");
        }

        family.PreferredCurrency = FamilyCurrencies.Normalize(request.PreferredCurrency);
        await _dbContext.SaveChangesAsync(ct);
        await Send.OkAsync(new Response(family.Id, family.PreferredCurrency), ct);
    }
}

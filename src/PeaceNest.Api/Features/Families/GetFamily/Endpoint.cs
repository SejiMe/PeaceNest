using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.Families.GetFamily;

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
        Get("/families/{familyId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Families"));
        Summary(summary =>
        {
            summary.Summary = "Get a family workspace.";
            summary.Description = "Returns one family workspace only when the authenticated user is an active family member.";
            summary.Responses[200] = "The family workspace was returned.";
            summary.Responses[403] = "The authenticated user is not a member of this family workspace.";
            summary.Responses[404] = "The family workspace was not found.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);

        var familyExists = await _dbContext.Families
            .AsNoTracking()
            .AnyAsync(family => family.Id == familyId, ct);

        if (!familyExists)
        {
            throw new NotFoundAppException("Family workspace was not found.");
        }

        var membership = await _dbContext.FamilyMembers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                member => member.FamilyId == familyId &&
                    member.UserId == user.Id &&
                    member.Status == FamilyMemberStatus.Active,
                ct);

        if (membership is null || !FamilyRolePermissions.CanViewFamily(membership.Role))
        {
            throw new AuthorizationAppException("You are not a member of this family workspace.");
        }

        var family = await _dbContext.Families
            .AsNoTracking()
            .Where(family => family.Id == familyId)
            .Select(family => new
            {
                family.Id,
                family.Name,
                family.Description,
                family.CreatedAt,
                MemberCount = family.Members.Count(member => member.Status == FamilyMemberStatus.Active)
            })
            .SingleAsync(ct);

        await Send.OkAsync(
            new Response(
                family.Id,
                family.Name,
                family.Description,
                membership.Role,
                family.MemberCount,
                family.CreatedAt),
            ct);
    }
}

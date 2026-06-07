using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Families.ListFamilies;

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
        Get("/families");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Families"));
        Summary(summary =>
        {
            summary.Summary = "List family workspaces.";
            summary.Description = "Lists family workspaces where the authenticated user is an active family member.";
            summary.Responses[200] = "The family workspaces were returned.";
            summary.Responses[401] = "A valid Supabase Google access token is required.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);

        var families = await _dbContext.FamilyMembers
            .AsNoTracking()
            .Where(member => member.UserId == user.Id && member.Status == FamilyMemberStatus.Active)
            .OrderBy(member => member.Family.Name)
            .Select(member => new FamilyWorkspaceResponse(
                member.FamilyId,
                member.Family.Name,
                member.Family.Description,
                member.Role,
                member.Family.Members.Count(innerMember => innerMember.Status == FamilyMemberStatus.Active),
                member.Family.CreatedAt))
            .ToListAsync(ct);

        await Send.OkAsync(new Response(families), ct);
    }
}

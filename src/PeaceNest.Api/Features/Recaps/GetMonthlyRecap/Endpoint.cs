using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.Recaps.GetMonthlyRecap;

public sealed class Endpoint : EndpointWithoutRequest<Response>
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
        Get("/families/{familyId:guid}/recaps/monthly/{year:int}/{month:int}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Recaps"));
        Summary(summary =>
        {
            summary.Summary = "Get a monthly recap.";
            summary.Description = "Returns one generated monthly recap for an authorized family member.";
            summary.Responses[200] = "The monthly recap was returned.";
            summary.Responses[400] = "The recap period was invalid.";
            summary.Responses[403] = "The authenticated user is not a member of this family workspace.";
            summary.Responses[404] = "The monthly recap was not found.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var period = MonthlyRecapPeriod.FromRoute(Route<int>("year"), Route<int>("month"));
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanViewFamily,
            "You do not have permission to view recaps for this family workspace.",
            ct);

        var recap = await _dbContext.Recaps
            .AsNoTracking()
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(
                candidate => candidate.FamilyId == familyId &&
                    candidate.PeriodType == RecapPeriodType.Monthly &&
                    candidate.PeriodStart == period.Start,
                ct);

        if (recap is null)
        {
            throw new NotFoundAppException("Monthly recap was not found.");
        }

        await Send.OkAsync(new Response(MonthlyRecapResponseProjection.FromRecap(recap)), ct);
    }
}

using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.Families.CreateFamily;

public sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly PeaceNestDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        PeaceNestDbContext dbContext,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Post("/families");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Families"));
        Summary(summary =>
        {
            summary.Summary = "Create a family workspace.";
            summary.Description = "Creates a family workspace and makes the authenticated user the Owner.";
            summary.Responses[201] = "The family workspace was created.";
            summary.Responses[400] = "The family workspace request was invalid.";
            summary.Responses[401] = "A valid Supabase Google access token is required.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        ValidateRequest(request);

        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        var now = _timeProvider.GetUtcNow();

        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedByUserId = user.Id
        };

        var membership = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FamilyId = family.Id,
            UserId = user.Id,
            Role = FamilyMemberRole.Owner,
            Status = FamilyMemberStatus.Active,
            JoinedAt = now
        };

        _dbContext.Families.Add(family);
        _dbContext.FamilyMembers.Add(membership);
        await _dbContext.SaveChangesAsync(ct);

        await Send.CreatedAtAsync(
            nameof(GetFamily.Endpoint),
            new { familyId = family.Id },
            new Response(
                family.Id,
                family.Name,
                family.Description,
                membership.Role,
                1,
                family.CreatedAt),
            cancellation: ct);
    }

    private static void ValidateRequest(Request request)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            failures.Add(new ValidationFailure("name", "Family workspace name is required."));
        }
        else if (request.Name.Trim().Length > 160)
        {
            failures.Add(new ValidationFailure("name", "Family workspace name must be 160 characters or fewer."));
        }

        if (request.Description?.Trim().Length > 500)
        {
            failures.Add(new ValidationFailure("description", "Family workspace description must be 500 characters or fewer."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationAppException("Family workspace request is invalid.", failures);
        }
    }
}

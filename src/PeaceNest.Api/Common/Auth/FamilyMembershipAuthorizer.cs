using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Common.Auth;

public sealed class FamilyMembershipAuthorizer
{
    private readonly PeaceNestDbContext _dbContext;

    public FamilyMembershipAuthorizer(PeaceNestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FamilyMember> RequireActiveMembershipAsync(
        Guid familyId,
        Guid userId,
        Func<FamilyMemberRole, bool> permission,
        string forbiddenMessage,
        CancellationToken cancellationToken)
    {
        var familyExists = await _dbContext.Families
            .AsNoTracking()
            .AnyAsync(family => family.Id == familyId, cancellationToken);

        if (!familyExists)
        {
            throw new NotFoundAppException("Family workspace was not found.");
        }

        var membership = await _dbContext.FamilyMembers
            .SingleOrDefaultAsync(
                member => member.FamilyId == familyId &&
                    member.UserId == userId &&
                    member.Status == FamilyMemberStatus.Active,
                cancellationToken);

        if (membership is null || !permission(membership.Role))
        {
            throw new AuthorizationAppException(forbiddenMessage);
        }

        return membership;
    }
}

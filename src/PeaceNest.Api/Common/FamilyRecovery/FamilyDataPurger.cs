using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.FamilyRecovery;

public sealed class FamilyDataPurger
{
    private readonly PeaceNestDbContext _dbContext;

    public FamilyDataPurger(PeaceNestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> PurgeExpiredFamilyAsync(
        Guid familyId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var isExpired = await _dbContext.FamilyRecoveryCodes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(code =>
                code.FamilyId == familyId &&
                code.Status == FamilyRecoveryCodeStatus.Active &&
                code.ExpiresAt <= now &&
                code.Family.DeletedAt != null,
                cancellationToken);

        if (!isExpired)
        {
            return false;
        }

        if (_dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            await PurgeRelationalAsync(familyId, cancellationToken);
        }
        else
        {
            await PurgeTrackedAsync(familyId, cancellationToken);
        }

        return true;
    }

    private async Task PurgeRelationalAsync(Guid familyId, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.ActivityLogs.Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Notifications.IgnoreQueryFilters().Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Reactions.Where(item =>
                (item.PlanId != null && item.Plan!.FamilyId == familyId) ||
                (item.CommentId != null && item.Comment!.Plan.FamilyId == familyId))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.RecapItems.Where(item => item.Recap.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.PlanParticipants.Where(item => item.Plan.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.PlanVotes.Where(item => item.Plan.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.GoalSteps.IgnoreQueryFilters().Where(item => item.Plan.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Comments.IgnoreQueryFilters().Where(item => item.Plan.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.WantNeedDetails.Where(item => item.Plan.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.MilestoneDetails.Where(item => item.Plan.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Memories.IgnoreQueryFilters().Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Recaps.IgnoreQueryFilters().Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.FamilyPlans.IgnoreQueryFilters().Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.PlanCategories.IgnoreQueryFilters().Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.FamilyJoinRequests.Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.FamilyJoinCodes.Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.FamilyInvitations.Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.FamilyMembers.Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.FamilyRecoveryCodes.Where(item => item.FamilyId == familyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Families.IgnoreQueryFilters().Where(item => item.Id == familyId).ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task PurgeTrackedAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var planIds = await _dbContext.FamilyPlans.IgnoreQueryFilters()
            .Where(item => item.FamilyId == familyId)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var commentIds = await _dbContext.Comments.IgnoreQueryFilters()
            .Where(item => planIds.Contains(item.PlanId))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var recapIds = await _dbContext.Recaps.IgnoreQueryFilters()
            .Where(item => item.FamilyId == familyId)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

        _dbContext.RemovePermanentlyRange(_dbContext.ActivityLogs.Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.Notifications.IgnoreQueryFilters().Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.Reactions.Where(item =>
            (item.PlanId != null && planIds.Contains(item.PlanId.Value)) ||
            (item.CommentId != null && commentIds.Contains(item.CommentId.Value))));
        _dbContext.RemovePermanentlyRange(_dbContext.RecapItems.Where(item => recapIds.Contains(item.RecapId)));
        _dbContext.RemovePermanentlyRange(_dbContext.PlanParticipants.Where(item => planIds.Contains(item.PlanId)));
        _dbContext.RemovePermanentlyRange(_dbContext.PlanVotes.Where(item => planIds.Contains(item.PlanId)));
        _dbContext.RemovePermanentlyRange(_dbContext.GoalSteps.IgnoreQueryFilters().Where(item => planIds.Contains(item.PlanId)));
        _dbContext.RemovePermanentlyRange(_dbContext.Comments.IgnoreQueryFilters().Where(item => planIds.Contains(item.PlanId)));
        _dbContext.RemovePermanentlyRange(_dbContext.WantNeedDetails.Where(item => planIds.Contains(item.PlanId)));
        _dbContext.RemovePermanentlyRange(_dbContext.MilestoneDetails.Where(item => planIds.Contains(item.PlanId)));
        _dbContext.RemovePermanentlyRange(_dbContext.Memories.IgnoreQueryFilters().Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.Recaps.IgnoreQueryFilters().Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.FamilyPlans.IgnoreQueryFilters().Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.PlanCategories.IgnoreQueryFilters().Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.FamilyJoinRequests.Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.FamilyJoinCodes.Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.FamilyInvitations.Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.FamilyMembers.Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.FamilyRecoveryCodes.Where(item => item.FamilyId == familyId));
        _dbContext.RemovePermanentlyRange(_dbContext.Families.IgnoreQueryFilters().Where(item => item.Id == familyId));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

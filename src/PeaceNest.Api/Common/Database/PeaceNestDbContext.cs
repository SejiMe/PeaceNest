using Microsoft.EntityFrameworkCore;

namespace PeaceNest.Api.Common.Database;

public sealed class PeaceNestDbContext : DbContext
{
    private readonly TimeProvider _timeProvider;

    public PeaceNestDbContext(
        DbContextOptions<PeaceNestDbContext> options,
        TimeProvider timeProvider)
        : base(options)
    {
        _timeProvider = timeProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyPeaceNestConventions();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ChangeTracker.ApplyPeaceNestPersistenceConventions(_timeProvider.GetUtcNow());
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ChangeTracker.ApplyPeaceNestPersistenceConventions(_timeProvider.GetUtcNow());
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Database;

namespace PeaceNest.Api.Tests.Unit.Common.Database;

public sealed class ChangeTrackerConventionTests
{
    [Fact]
    public void ApplyPeaceNestPersistenceConventions_SetsAuditValuesForAddedEntity()
    {
        using var dbContext = CreateDbContext();
        var now = new DateTimeOffset(2026, 6, 7, 0, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Family Vacation" };

        dbContext.TestEntities.Add(entity);
        dbContext.ChangeTracker.ApplyPeaceNestPersistenceConventions(now);

        Assert.Equal(now, entity.CreatedAt);
        Assert.Equal(now, entity.UpdatedAt);
        Assert.Equal(1, entity.Version);
    }

    [Fact]
    public void ApplyPeaceNestPersistenceConventions_AssignsVersion7GuidForMarkedAddedEntity()
    {
        using var dbContext = CreateDbContext();
        var now = new DateTimeOffset(2026, 6, 7, 0, 0, 0, TimeSpan.Zero);
        var entity = new TimeOrderedTestEntity { Name = "Vote cast" };

        dbContext.TimeOrderedTestEntities.Add(entity);
        dbContext.ChangeTracker.ApplyPeaceNestPersistenceConventions(now);

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal('7', entity.Id.ToString("D")[14]);
    }

    [Fact]
    public void ApplyPeaceNestPersistenceConventions_DoesNotReplaceExplicitGuidForUnmarkedAddedEntity()
    {
        using var dbContext = CreateDbContext();
        var now = new DateTimeOffset(2026, 6, 7, 0, 0, 0, TimeSpan.Zero);
        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id, Name = "Family root" };

        dbContext.TestEntities.Add(entity);
        dbContext.ChangeTracker.ApplyPeaceNestPersistenceConventions(now);

        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void ApplyPeaceNestPersistenceConventions_UpdatesAuditAndVersionForModifiedEntity()
    {
        using var dbContext = CreateDbContext();
        var createdAt = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var now = new DateTimeOffset(2026, 6, 7, 0, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Tuition",
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            Version = 3
        };

        dbContext.Attach(entity);
        entity.Name = "Tuition Plan";
        dbContext.ChangeTracker.ApplyPeaceNestPersistenceConventions(now);

        Assert.Equal(createdAt, entity.CreatedAt);
        Assert.Equal(now, entity.UpdatedAt);
        Assert.Equal(4, entity.Version);
        Assert.False(dbContext.Entry(entity).Property(nameof(IAuditableEntity.CreatedAt)).IsModified);
    }

    [Fact]
    public void ApplyPeaceNestPersistenceConventions_ConvertsDeleteToSoftDelete()
    {
        using var dbContext = CreateDbContext();
        var now = new DateTimeOffset(2026, 6, 7, 0, 0, 0, TimeSpan.Zero);
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Old plan",
            Version = 1
        };

        dbContext.Attach(entity);
        dbContext.Remove(entity);
        dbContext.ChangeTracker.ApplyPeaceNestPersistenceConventions(now);

        Assert.Equal(EntityState.Modified, dbContext.Entry(entity).State);
        Assert.Equal(now, entity.DeletedAt);
        Assert.Equal(now, entity.UpdatedAt);
        Assert.Equal(2, entity.Version);
    }

    [Fact]
    public void ApplyPeaceNestConventions_ConfiguresVersionAsConcurrencyToken()
    {
        using var dbContext = CreateDbContext();

        var versionProperty = dbContext.Model
            .FindEntityType(typeof(TestEntity))!
            .FindProperty(nameof(IConcurrencyTrackedEntity.Version));

        Assert.NotNull(versionProperty);
        Assert.True(versionProperty.IsConcurrencyToken);
    }

    [Fact]
    public async Task ApplyPeaceNestConventions_FiltersSoftDeletedEntities()
    {
        using var dbContext = CreateDbContext();
        dbContext.TestEntities.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "Visible plan" },
            new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "Deleted plan",
                DeletedAt = new DateTimeOffset(2026, 6, 7, 0, 0, 0, TimeSpan.Zero)
            });

        await dbContext.SaveChangesAsync();

        var visibleEntities = await dbContext.TestEntities.ToListAsync();

        Assert.Single(visibleEntities);
        Assert.Equal("Visible plan", visibleEntities[0].Name);
    }

    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        public DbSet<TimeOrderedTestEntity> TimeOrderedTestEntities => Set<TimeOrderedTestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>().HasKey(entity => entity.Id);
            modelBuilder.Entity<TimeOrderedTestEntity>().HasKey(entity => entity.Id);
            modelBuilder.ApplyPeaceNestConventions();
        }
    }

    private sealed class TestEntity : IAuditableEntity, ISoftDeletableEntity, IConcurrencyTrackedEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        public int Version { get; set; }
    }

    private sealed class TimeOrderedTestEntity : IUsesVersion7Guid
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PeaceNest.Api.Common.Database;

public sealed class PeaceNestDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PeaceNestDbContext>
{
    public PeaceNestDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__PeaceNest")
            ?? Environment.GetEnvironmentVariable("PEACENEST_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=peacenest_dev;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<PeaceNestDbContext>()
            .UseNpgsql(connectionString, PeaceNestDatabaseExtensions.ConfigurePeaceNestNpgsqlOptions)
            .Options;

        return new PeaceNestDbContext(options, TimeProvider.System);
    }
}

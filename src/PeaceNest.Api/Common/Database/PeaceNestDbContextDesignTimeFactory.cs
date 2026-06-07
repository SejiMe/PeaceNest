using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PeaceNest.Api.Common.Database;

public sealed class PeaceNestDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PeaceNestDbContext>
{
    public PeaceNestDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets(typeof(PeaceNestDbContextDesignTimeFactory).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("PeaceNestMigration")
            ?? configuration.GetConnectionString(DatabaseOptions.ConnectionStringName)
            ?? Environment.GetEnvironmentVariable("PEACENEST_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=peacenest_dev;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<PeaceNestDbContext>()
            .UseNpgsql(connectionString, PeaceNestDatabaseExtensions.ConfigurePeaceNestNpgsqlOptions)
            .Options;

        return new PeaceNestDbContext(options, TimeProvider.System);
    }
}

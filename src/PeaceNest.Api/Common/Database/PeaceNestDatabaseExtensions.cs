using Microsoft.EntityFrameworkCore;

namespace PeaceNest.Api.Common.Database;

public static class PeaceNestDatabaseExtensions
{
    public static IServiceCollection AddPeaceNestDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateOnStart();

        services.AddDbContext<PeaceNestDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString(DatabaseOptions.ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                if (environment.IsProduction())
                {
                    throw new InvalidOperationException(
                        $"ConnectionStrings:{DatabaseOptions.ConnectionStringName} is required in production.");
                }

                connectionString = "Host=localhost;Port=5432;Database=peacenest_dev;Username=postgres;Password=postgres";
            }

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(PeaceNestDbContext).Assembly.FullName);
            });

            var databaseOptions = configuration
                .GetSection(DatabaseOptions.SectionName)
                .Get<DatabaseOptions>() ?? new DatabaseOptions();

            if (!environment.IsProduction() && databaseOptions.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }
}

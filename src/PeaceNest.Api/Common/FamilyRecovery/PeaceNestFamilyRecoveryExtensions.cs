namespace PeaceNest.Api.Common.FamilyRecovery;

public static class PeaceNestFamilyRecoveryExtensions
{
    public static IServiceCollection AddPeaceNestFamilyRecovery(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<FamilyRecoveryOptions>()
            .Bind(configuration.GetSection(FamilyRecoveryOptions.SectionName))
            .Validate(options => options.LifetimeDays is > 0 and <= 365,
                "FamilyRecovery:LifetimeDays must be between 1 and 365.")
            .Validate(options => options.SweepIntervalMinutes is > 0 and <= 1440,
                "FamilyRecovery:SweepIntervalMinutes must be between 1 and 1440.")
            .Validate(options => options.ClaimLeaseMinutes is > 0 and <= 1440,
                "FamilyRecovery:ClaimLeaseMinutes must be between 1 and 1440.")
            .Validate(options => options.BatchSize is > 0 and <= 100,
                "FamilyRecovery:BatchSize must be between 1 and 100.")
            .ValidateOnStart();

        services.AddSingleton<FamilyRecoveryCodeService>();
        services.AddSingleton<FamilyRecoveryPolicy>();
        services.AddScoped<FamilyDataPurger>();
        services.AddHostedService<FamilyRecoveryPurgeWorker>();
        return services;
    }
}

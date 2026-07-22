namespace PeaceNest.Api.Common.JoinCodes;

public static class PeaceNestJoinCodeExtensions
{
    public static IServiceCollection AddPeaceNestJoinCodes(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<JoinCodePolicyOptions>()
            .Bind(configuration.GetSection(JoinCodePolicyOptions.SectionName))
            .Validate(options => options.LifetimeMinutes is > 0 and <= 1440,
                "JoinCodes:LifetimeMinutes must be between 1 and 1440.")
            .Validate(options => options.RequestLifetimeDays is > 0 and <= 30,
                "JoinCodes:RequestLifetimeDays must be between 1 and 30.")
            .Validate(options => options.MaxRequestsPerCode is > 0 and <= 100,
                "JoinCodes:MaxRequestsPerCode must be between 1 and 100.")
            .ValidateOnStart();

        services.AddSingleton<FamilyJoinCodeService>();
        services.AddSingleton<JoinCodePolicy>();
        return services;
    }
}

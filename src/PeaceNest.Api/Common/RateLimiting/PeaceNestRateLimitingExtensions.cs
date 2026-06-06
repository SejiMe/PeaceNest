using System.Threading.RateLimiting;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Common.RateLimiting;

public static class PeaceNestRateLimitingExtensions
{
    public static IServiceCollection AddPeaceNestRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimitOptions = configuration
            .GetSection(PeaceNestRateLimitingOptions.SectionName)
            .Get<PeaceNestRateLimitingOptions>() ?? new PeaceNestRateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = CreatePartitionedLimiter(rateLimitOptions.Global);

            options.AddPolicy(
                RateLimitPolicyNames.Auth,
                httpContext => CreatePartition(httpContext, rateLimitOptions.Auth));
            options.AddPolicy(
                RateLimitPolicyNames.Write,
                httpContext => CreatePartition(httpContext, rateLimitOptions.Write));
            options.AddPolicy(
                RateLimitPolicyNames.Invite,
                httpContext => CreatePartition(httpContext, rateLimitOptions.Invite));
            options.AddPolicy(
                RateLimitPolicyNames.RecapGeneration,
                httpContext => CreatePartition(httpContext, rateLimitOptions.RecapGeneration));
            options.AddPolicy(
                RateLimitPolicyNames.TestingTight,
                httpContext => CreatePartition(httpContext, new RateLimitRuleOptions
                {
                    PermitLimit = 1,
                    WindowSeconds = 60
                }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                await ProblemDetailsResponseWriter.WriteAsync(
                    context.HttpContext,
                    ApiErrorDescriptor.RateLimit("PeaceNest needs a short pause before trying that again."),
                    context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>(),
                    cancellationToken);
            };
        });

        return services;
    }

    private static PartitionedRateLimiter<HttpContext> CreatePartitionedLimiter(RateLimitRuleOptions ruleOptions) =>
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext => CreatePartition(httpContext, ruleOptions));

    private static RateLimitPartition<string> CreatePartition(
        HttpContext httpContext,
        RateLimitRuleOptions ruleOptions)
    {
        var partitionKey = RateLimitPartitionKeyResolver.Resolve(httpContext);

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = ruleOptions.PermitLimit,
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(ruleOptions.WindowSeconds)
            });
    }
}

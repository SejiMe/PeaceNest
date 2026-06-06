using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Common.Auth;

public static class PeaceNestAuthenticationExtensions
{
    private const string TestingIssuer = "https://testing.supabase.co/auth/v1";

    public static IServiceCollection AddPeaceNestAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddOptions<SupabaseAuthOptions>()
            .Bind(configuration.GetSection(SupabaseAuthOptions.SectionName))
            .Validate(
                options => !environment.IsProduction() || !string.IsNullOrWhiteSpace(options.ProjectUrl),
                "Authentication:Supabase:ProjectUrl is required in production.")
            .ValidateOnStart();

        var supabaseOptions = configuration
            .GetSection(SupabaseAuthOptions.SectionName)
            .Get<SupabaseAuthOptions>() ?? new SupabaseAuthOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
                ConfigureJwtBearer(jwtOptions, supabaseOptions, environment);
            });

        services.AddAuthorization();

        return services;
    }

    private static void ConfigureJwtBearer(
        JwtBearerOptions jwtOptions,
        SupabaseAuthOptions supabaseOptions,
        IHostEnvironment environment)
    {
        jwtOptions.MapInboundClaims = false;
        jwtOptions.IncludeErrorDetails = environment.IsDevelopment() || environment.IsEnvironment("Testing");

        var issuer = ConfigureTokenValidation(jwtOptions, supabaseOptions, environment);

        jwtOptions.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                var error = ApiErrorDescriptor.Authentication(
                    ErrorCodes.AuthenticationRequired,
                    "A valid Supabase access token is required.");

                await ProblemDetailsResponseWriter.WriteAsync(
                    context.HttpContext,
                    error,
                    context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>(),
                    context.HttpContext.RequestAborted);
            },
            OnForbidden = async context =>
            {
                var error = ApiErrorDescriptor.Authorization(
                    ErrorCodes.AuthorizationDenied,
                    "You do not have permission to access this family space.");

                await ProblemDetailsResponseWriter.WriteAsync(
                    context.HttpContext,
                    error,
                    context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>(),
                    context.HttpContext.RequestAborted);
            }
        };

        jwtOptions.TokenValidationParameters.NameClaimType = AuthClaimTypes.Subject;
        jwtOptions.TokenValidationParameters.RoleClaimType = AuthClaimTypes.Role;
        jwtOptions.TokenValidationParameters.ValidIssuer ??= issuer;
        jwtOptions.TokenValidationParameters.ValidAudience ??= supabaseOptions.Audience;
    }

    private static string ConfigureTokenValidation(
        JwtBearerOptions jwtOptions,
        SupabaseAuthOptions supabaseOptions,
        IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing") && !string.IsNullOrWhiteSpace(supabaseOptions.TestingSigningKey))
        {
            jwtOptions.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = TestingIssuer,
                ValidAudience = supabaseOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseOptions.TestingSigningKey))
            };

            return TestingIssuer;
        }

        if (!string.IsNullOrWhiteSpace(supabaseOptions.ProjectUrl))
        {
            var issuer = SupabaseJwtIssuer.FromProjectUrl(supabaseOptions.ProjectUrl);
            jwtOptions.Authority = issuer;
            jwtOptions.Audience = supabaseOptions.Audience;
            jwtOptions.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = issuer,
                ValidAudience = supabaseOptions.Audience
            };

            return issuer;
        }

        return TestingIssuer;
    }
}

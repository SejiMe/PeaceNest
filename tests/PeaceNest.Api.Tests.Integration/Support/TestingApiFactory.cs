using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PeaceNest.Api.Tests.Integration.Support;

public sealed class TestingApiFactory : WebApplicationFactory<Program>
{
    public const string SigningKey = "PeaceNest testing signing key with enough bytes";
    public const string Issuer = "https://testing.supabase.co/auth/v1";
    public const string Audience = "authenticated";

    private readonly IReadOnlyDictionary<string, string?> _configuration;

    public TestingApiFactory()
    {
        _configuration = new Dictionary<string, string?>();
    }

    private TestingApiFactory(IReadOnlyDictionary<string, string?> configuration)
    {
        _configuration = configuration;
    }

    public static TestingApiFactory WithConfiguration(IReadOnlyDictionary<string, string?> configuration) =>
        new(configuration);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var configuration = new Dictionary<string, string?>
            {
                ["Authentication:Supabase:Audience"] = Audience,
                ["Authentication:Supabase:TestingSigningKey"] = SigningKey
            };

            foreach (var item in _configuration)
            {
                configuration[item.Key] = item.Value;
            }

            configurationBuilder.AddInMemoryCollection(configuration);
        });
        builder.ConfigureServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
            });
        });
    }
}

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PeaceNest.Api.Common.Database;

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

    public static TestingApiFactory WithIsolatedDatabase() =>
        new(new Dictionary<string, string?>
        {
            ["Testing:UseInMemoryDatabase"] = "true",
            ["Testing:DatabaseName"] = $"peacenest-tests-{Guid.NewGuid():N}"
        });

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
            if (_configuration.TryGetValue("Testing:UseInMemoryDatabase", out var useInMemoryDatabase) &&
                string.Equals(useInMemoryDatabase, "true", StringComparison.OrdinalIgnoreCase))
            {
                var databaseName = _configuration.TryGetValue("Testing:DatabaseName", out var configuredName)
                    ? configuredName
                    : $"peacenest-tests-{Guid.NewGuid():N}";

                services.RemoveAll<DbContextOptions<PeaceNestDbContext>>();
                var inMemoryProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<PeaceNestDbContext>(options =>
                {
                    options
                        .UseInMemoryDatabase(databaseName!)
                        .UseInternalServiceProvider(inMemoryProvider);
                });
            }

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

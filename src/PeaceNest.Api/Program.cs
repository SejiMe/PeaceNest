using FastEndpoints;
using FastEndpoints.Swagger;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.FamilyRecovery;
using PeaceNest.Api.Common.JoinCodes;
using PeaceNest.Api.Common.RateLimiting;
using PeaceNest.Api.Common.Security;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<FamilyMembershipAuthorizer>();
builder.Services.AddSingleton<InvitationTokenService>();
builder.Services.AddPeaceNestAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddPeaceNestDatabase(builder.Configuration, builder.Environment);
builder.Services.AddPeaceNestJoinCodes(builder.Configuration);
builder.Services.AddPeaceNestFamilyRecovery(builder.Configuration);
builder.Services.AddPeaceNestRateLimiting(builder.Configuration);
builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(options =>
    {
        options.ExcludeNonFastEndpoints = true;
        options.DocumentSettings = document =>
        {
            document.DocumentName = "v1";
            document.Title = "PeaceNest API";
            document.Version = "v1";
        };
    });

builder.Services.AddHealthChecks();

if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowAllOrigin",
            policy =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
            });
    });
}

var app = builder.Build();

if (app.Environment.IsEnvironment("Testing"))
{
    app.MapGet("/testing/errors/{kind}", (string kind) =>
    {
        throw kind.ToLowerInvariant() switch
        {
            "validation" => new ValidationAppException(
                "Validation failed.",
                [new("name", "Name is required.")]),
            "authentication" => new AuthenticationAppException("Authentication is required."),
            "authorization" => new AuthorizationAppException("You are not allowed to access this family space."),
            "not-found" => new NotFoundAppException("Family plan was not found."),
            "conflict" => new ConflictAppException("Family plan was updated by someone else."),
            "domain" => new DomainRuleAppException("This family plan cannot be completed yet."),
            "external-provider" => new ExternalProviderAppException("Supabase is unavailable."),
            _ => new InvalidOperationException("Unexpected diagnostic failure.")
        };
    }).ExcludeFromDescription();

    app.MapGet("/testing/rate-limit", () => Results.Ok(new { status = "ok" }))
        .RequireRateLimiting(RateLimitPolicyNames.TestingTight)
        .ExcludeFromDescription();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseCors("AllowAllOrigin");
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwaggerGen(options =>
    {
        options.Path = "/openapi/{documentName}.json";
    });

    app.MapScalarApiReference("/scalar", options =>
    {
        options
            .WithTitle("PeaceNest API")
            .DisableDefaultFonts()
            .DisableAgent();
    });
}

app.Run();

public partial class Program;

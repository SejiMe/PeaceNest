using FastEndpoints;
using FastEndpoints.Swagger;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

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

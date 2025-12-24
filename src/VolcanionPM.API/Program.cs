using VolcanionPM.API.Middleware;
using VolcanionPM.API.Authorization.Requirements;
using VolcanionPM.API.Authorization.Handlers;
using VolcanionPM.API.Configuration;
using VolcanionPM.Application;
using VolcanionPM.Infrastructure;
using VolcanionPM.Infrastructure.Extensions;
using VolcanionPM.Infrastructure.Persistence;
using Serilog;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with enrichers
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "VolcanionPM")
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/volcanionpm-.log", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{MachineName}] [{EnvironmentName}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_000_000)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Bind Security Settings
var securitySettings = builder.Configuration.GetSection("Security").Get<SecuritySettings>() ?? new SecuritySettings();
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("Security"));

// Register Application Layer
builder.Services.AddApplicationServices();

// Register Infrastructure Layer
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "postgres" })
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), tags: new[] { "api" });

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("VolcanionPM.API"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        // TODO: Phase 3 - Add .AddRuntimeInstrumentation() when package available
        .AddPrometheusExporter())
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());
        // TODO: Phase 3 - Add .AddEntityFrameworkCoreInstrumentation() when EF Core is implemented

// Add CORS with security configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        var corsSettings = securitySettings.Cors;
        
        if (corsSettings.AllowedOrigins.Length > 0)
        {
            policy.WithOrigins(corsSettings.AllowedOrigins);
        }
        else
        {
            policy.AllowAnyOrigin();
        }

        if (corsSettings.AllowedMethods.Length > 0)
        {
            policy.WithMethods(corsSettings.AllowedMethods);
        }
        else
        {
            policy.AllowAnyMethod();
        }

        if (corsSettings.AllowedHeaders.Length > 0)
        {
            policy.WithHeaders(corsSettings.AllowedHeaders);
        }
        else
        {
            policy.AllowAnyHeader();
        }

        if (corsSettings.ExposedHeaders.Length > 0)
        {
            policy.WithExposedHeaders(corsSettings.ExposedHeaders);
        }

        if (corsSettings.AllowCredentials)
        {
            policy.AllowCredentials();
        }

        if (corsSettings.MaxAgeSeconds > 0)
        {
            policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.MaxAgeSeconds));
        }
    });
});

// Add Rate Limiting
builder.Services.AddRateLimiting();

// Register Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, CanEditProjectHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CanDeleteProjectHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CanAssignTaskHandler>();
builder.Services.AddScoped<IAuthorizationHandler, IsResourceOwnerHandler>();

// Configure Authorization Policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanEditProject", policy =>
        policy.Requirements.Add(new CanEditProjectRequirement()))
    .AddPolicy("CanDeleteProject", policy =>
        policy.Requirements.Add(new CanDeleteProjectRequirement()))
    .AddPolicy("CanAssignTask", policy =>
        policy.Requirements.Add(new CanAssignTaskRequirement()))
    .AddPolicy("IsResourceOwner", policy =>
        policy.Requirements.Add(new IsResourceOwnerRequirement()));

var app = builder.Build();

// Initialize database (apply migrations and seed data in development)
// Skip initialization in Testing environment (used by integration tests)
if (!app.Environment.IsEnvironment("Testing"))
{
    await app.InitializeDatabaseAsync(isDevelopment: app.Environment.IsDevelopment());
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Middleware pipeline
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<InputValidationMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("DefaultPolicy");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString(),
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            }),
            totalDuration = report.TotalDuration.ToString()
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("api")
});

// Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();

try
{
    Log.Information("Starting Volcanion PM API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

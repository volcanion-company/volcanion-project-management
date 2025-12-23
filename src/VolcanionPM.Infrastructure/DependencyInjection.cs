using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VolcanionPM.Infrastructure.Persistence;
using VolcanionPM.Infrastructure.Repositories;
using VolcanionPM.Infrastructure.Services;
using VolcanionPM.Application.Common.Interfaces;

namespace VolcanionPM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database Contexts
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=volcanion_pm;Username=postgres;Password=postgres",
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddDbContext<ReadDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=volcanion_pm;Username=postgres;Password=postgres",
                b => b.EnableRetryOnFailure())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISprintRepository, SprintRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<IRiskRepository, RiskRepository>();
        services.AddScoped<IIssueRepository, IssueRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IResourceAllocationRepository, ResourceAllocationRepository>();

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:Configuration"] ?? "localhost:6379";
            options.InstanceName = configuration["Redis:InstanceName"] ?? "VolcanionPM_";
        });
        services.AddScoped<ICacheService, RedisCacheService>();

        // JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "YourSuperSecretKeyHere1234567890123456");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "VolcanionPM",
                ValidAudience = jwtSettings["Audience"] ?? "VolcanionPM",
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Token Service
        services.AddScoped<ITokenService, TokenService>();
        
        // Password Hasher
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VolcanionPM.Application.DTOs.Auth;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;
using VolcanionPM.Infrastructure.Persistence;
using Xunit;

namespace VolcanionPM.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests with common setup and utilities.
/// </summary>
public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory _factory;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        // InMemory database is created automatically on first use - no need to call EnsureCreated()
    }

    /// <summary>
    /// Get database context for test data setup.
    /// </summary>
    protected ApplicationDbContext GetDbContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// Clear all data from database (call between tests for isolation).
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        var db = GetDbContext();
        
        // Clear all entities
        db.TaskComments.RemoveRange(db.TaskComments);
        db.ResourceAllocations.RemoveRange(db.ResourceAllocations);
        db.Documents.RemoveRange(db.Documents);
        db.Issues.RemoveRange(db.Issues);
        db.Risks.RemoveRange(db.Risks);
        db.TimeEntries.RemoveRange(db.TimeEntries);
        db.Tasks.RemoveRange(db.Tasks);
        db.Sprints.RemoveRange(db.Sprints);
        db.Projects.RemoveRange(db.Projects);
        db.Users.RemoveRange(db.Users);
        db.Organizations.RemoveRange(db.Organizations);
        
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Create a test organization in the database.
    /// </summary>
    protected async Task<Organization> CreateTestOrganizationAsync()
    {
        var db = GetDbContext();
        
        var organization = Organization.Create(
            name: "Test Organization",
            description: "Test Org Description",
            createdBy: "system"
        );
        
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();
        
        return organization;
    }

    /// <summary>
    /// Create a test user in the database.
    /// </summary>
    protected async Task<User> CreateTestUserAsync(
        Guid organizationId, 
        UserRole role = UserRole.Developer,
        string email = "test@example.com",
        string password = "HashedPassword123")
    {
        var db = GetDbContext();
        
        var user = User.Create(
            firstName: "Test",
            lastName: "User",
            email: email,
            passwordHash: password,
            organizationId: organizationId,
            role: role,
            phoneNumber: null,
            createdBy: "system"
        );
        
        db.Users.Add(user);
        await db.SaveChangesAsync();
        
        return user;
    }

    /// <summary>
    /// Create a test project in the database.
    /// </summary>
    protected async Task<Project> CreateTestProjectAsync(
        Guid organizationId, 
        string name = "Test Project",
        string code = "TEST-001")
    {
        var db = GetDbContext();
        
        var project = Project.Create(
            name: name,
            code: code,
            organizationId: organizationId,
            projectManagerId: Guid.NewGuid(), // Dummy project manager
            dateRange: DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(90)),
            budget: Money.Create(100000, "USD"),
            priority: ProjectPriority.Medium,
            description: "Test project description",
            createdBy: "system"
        );
        
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        
        return project;
    }

    /// <summary>
    /// Register a new user via API and return the user ID.
    /// </summary>
    protected async Task<(Guid UserId, string Token)> RegisterUserAsync(
        string email = "testuser@example.com",
        string password = "SecureP@ss123",
        UserRole role = UserRole.Developer)
    {
        // Create organization first
        var org = await CreateTestOrganizationAsync();
        
        var registerRequest = new
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = password,
            Role = role,
            OrganizationId = org.Id
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        
        return (result!.User.Id, result.AccessToken);
    }

    /// <summary>
    /// Login a user via API and return the access token.
    /// </summary>
    protected async Task<string> LoginUserAsync(string email, string password)
    {
        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        
        return result!.AccessToken;
    }

    /// <summary>
    /// Set the Authorization header with the given token.
    /// </summary>
    protected void SetAuthToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Clear the Authorization header.
    /// </summary>
    protected void ClearAuthToken()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}

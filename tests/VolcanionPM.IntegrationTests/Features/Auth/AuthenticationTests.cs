using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VolcanionPM.Application.DTOs.Auth;
using VolcanionPM.Domain.Enums;
using VolcanionPM.IntegrationTests.Infrastructure;
using Xunit;

namespace VolcanionPM.IntegrationTests.Features.Auth;

public class AuthenticationTests : IntegrationTestBase
{
    public AuthenticationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        await ClearDatabaseAsync();
        var org = await CreateTestOrganizationAsync();
        
        var request = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = org.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.User.Id.Should().NotBeEmpty();
        result.User.Email.Should().Be("john.doe@example.com");
        result.User.FirstName.Should().Be("John");
        result.User.LastName.Should().Be("Doe");
        result.User.Role.Should().Be("Developer");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn400()
    {
        // Arrange
        await ClearDatabaseAsync();
        var org = await CreateTestOrganizationAsync();
        await CreateTestUserAsync(org.Id, email: "duplicate@example.com");
        
        var request = new
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "duplicate@example.com", // Already exists
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = org.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidPassword_ShouldReturn400()
    {
        // Arrange
        await ClearDatabaseAsync();
        var org = await CreateTestOrganizationAsync();
        
        var request = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "weak", // Too short, missing requirements
            Role = UserRole.Developer,
            OrganizationId = org.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        await ClearDatabaseAsync();
        var (userId, _) = await RegisterUserAsync("user@example.com", "SecureP@ss123");
        
        var loginRequest = new
        {
            Email = "user@example.com",
            Password = "SecureP@ss123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Id.Should().Be(userId);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturn401()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var loginRequest = new
        {
            Email = "nonexistent@example.com",
            Password = "SecureP@ss123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401()
    {
        // Arrange
        await ClearDatabaseAsync();
        await RegisterUserAsync("user@example.com", "SecureP@ss123");
        
        var loginRequest = new
        {
            Email = "user@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        await ClearDatabaseAsync();
        var (_, token) = await RegisterUserAsync("user@example.com", "SecureP@ss123");
        
        // Login to get refresh token
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "user@example.com",
            Password = "SecureP@ss123"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        
        var refreshRequest = new
        {
            Token = token,
            RefreshToken = loginResult!.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.AccessToken.Should().NotBe(token); // New token should be different
    }

    [Fact]
    public async Task RefreshToken_WithInvalidRefreshToken_ShouldReturn401()
    {
        // Arrange
        await ClearDatabaseAsync();
        var (_, token) = await RegisterUserAsync("user@example.com", "SecureP@ss123");
        
        var refreshRequest = new
        {
            Token = token,
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturn401()
    {
        // Arrange
        await ClearDatabaseAsync();
        ClearAuthToken();

        // Act - Try to access protected endpoint without token
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ShouldReturn200()
    {
        // Arrange
        await ClearDatabaseAsync();
        var (_, token) = await RegisterUserAsync("user@example.com", "SecureP@ss123", UserRole.ProjectManager);
        SetAuthToken(token);

        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredToken_ShouldReturn401()
    {
        // Arrange
        await ClearDatabaseAsync();
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.expired.token";
        SetAuthToken(expiredToken);

        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.DTOs.Projects;
using VolcanionPM.Application.Features.Projects.Queries.GetById;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Tests.Features.Projects.Queries;

public class GetProjectByIdQueryHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<GetProjectByIdQueryHandler>> _loggerMock;
    private readonly GetProjectByIdQueryHandler _handler;

    public GetProjectByIdQueryHandlerTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<GetProjectByIdQueryHandler>>();

        _handler = new GetProjectByIdQueryHandler(
            _projectRepositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithCachedProject_ShouldReturnFromCache()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var query = new GetProjectByIdQuery(projectId);

        var cachedDto = new ProjectDto
        {
            Id = projectId,
            Name = "Cached Project",
            Code = "CACHE-001",
            Status = "Active",
            Priority = "High"
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<ProjectDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(projectId);
        result.Data.Name.Should().Be("Cached Project");

        // Verify repository was not called
        _projectRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoCacheHit_ShouldQueryDatabase()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var query = new GetProjectByIdQuery(projectId);

        var project = Project.Create(
            "Test Project",
            "TEST-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(60)),
            Money.Create(100000, "USD"),
            ProjectPriority.High,
            "Test Description",
            "test@example.com");

        _cacheServiceMock
            .Setup(x => x.GetAsync<ProjectDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectDto?)null);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _cacheServiceMock
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<ProjectDto>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(project.Id);
        result.Data.Name.Should().Be("Test Project");
        result.Data.Code.Should().Be("TEST-001");

        _projectRepositoryMock.Verify(
            x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()),
            Times.Once);

        _cacheServiceMock.Verify(
            x => x.SetAsync(It.IsAny<string>(), It.IsAny<ProjectDto>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProject_ShouldReturnFailure()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var query = new GetProjectByIdQuery(projectId);

        _cacheServiceMock
            .Setup(x => x.GetAsync<ProjectDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectDto?)null);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");

        _cacheServiceMock.Verify(
            x => x.SetAsync(It.IsAny<string>(), It.IsAny<ProjectDto>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var query = new GetProjectByIdQuery(projectId);

        _cacheServiceMock
            .Setup(x => x.GetAsync<ProjectDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectDto?)null);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving project")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldLogWarning()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var query = new GetProjectByIdQuery(projectId);

        _cacheServiceMock
            .Setup(x => x.GetAsync<ProjectDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectDto?)null);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCorrectCacheKey()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var query = new GetProjectByIdQuery(projectId);
        string? capturedCacheKey = null;

        _cacheServiceMock
            .Setup(x => x.GetAsync<ProjectDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((key, _) => capturedCacheKey = key)
            .ReturnsAsync((ProjectDto?)null);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedCacheKey.Should().NotBeNull();
        capturedCacheKey.Should().Contain(projectId.ToString());
    }
}

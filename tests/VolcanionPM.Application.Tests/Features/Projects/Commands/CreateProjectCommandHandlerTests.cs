using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Features.Projects.Commands.Create;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Tests.Features.Projects.Commands;

public class CreateProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateProjectCommandHandler>> _loggerMock;
    private readonly CreateProjectCommandHandler _handler;

    public CreateProjectCommandHandlerTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateProjectCommandHandler>>();
        
        _handler = new CreateProjectCommandHandler(
            _projectRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProject()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "E-Commerce Platform",
            Code = "ECOM-001",
            Description = "New e-commerce platform",
            Priority = "High",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(90),
            BudgetAmount = 100000m,
            BudgetCurrency = "USD",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        _projectRepositoryMock
            .Setup(x => x.GetByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        _projectRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);
        result.Data.Code.Should().Be(command.Code);
        result.Data.Status.Should().Be("Planning");

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Project>(p => p.Name == command.Name && p.Code == command.Code), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateCode_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "DUPLICATE-001",
            Priority = "Medium",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        var existingProject = Project.Create(
            "Existing Project",
            command.Code,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30)),
            Money.Create(50000, "USD"),
            ProjectPriority.Medium,
            "Existing project",
            "system");

        _projectRepositoryMock
            .Setup(x => x.GetByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPriority_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = "InvalidPriority",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        _projectRepositoryMock
            .Setup(x => x.GetByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid priority");

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoBudget_ShouldCreateWithDefaultBudget()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-002",
            Priority = "Low",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com",
            BudgetAmount = null,
            BudgetCurrency = null
        };

        _projectRepositoryMock
            .Setup(x => x.GetByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoDateRange_ShouldCreateWithDefaultDateRange()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-003",
            Priority = "Medium",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com",
            StartDate = null,
            EndDate = null
        };

        _projectRepositoryMock
            .Setup(x => x.GetByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-004",
            Priority = "High",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        _projectRepositoryMock
            .Setup(x => x.GetByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating project")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentPriorities_ShouldCreateSuccessfully()
    {
        // Arrange
        var priorities = new[] { "Low", "Medium", "High", "Critical" };

        foreach (var priority in priorities)
        {
            var command = new CreateProjectCommand
            {
                Name = $"Test Project {priority}",
                Code = $"TEST-{priority}",
                Priority = priority,
                OrganizationId = Guid.NewGuid(),
                CreatedBy = "test@example.com"
            };

            _projectRepositoryMock
                .Setup(x => x.GetByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}

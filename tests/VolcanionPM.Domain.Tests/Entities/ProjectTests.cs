using FluentAssertions;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Domain.Tests.Entities;

public class ProjectTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProject()
    {
        // Arrange
        var name = "E-Commerce Platform";
        var code = "ECOM-001";
        var orgId = Guid.NewGuid();
        var pmId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(90));
        var budget = Money.Create(100000, "USD");
        var priority = ProjectPriority.High;

        // Act
        var project = Project.Create(name, code, orgId, pmId, dateRange, budget, priority, "Test project", "TestUser");

        // Assert
        project.Should().NotBeNull();
        project.Id.Should().NotBeEmpty();
        project.Name.Should().Be(name);
        project.Code.Should().Be(code.ToUpperInvariant());
        project.OrganizationId.Should().Be(orgId);
        project.ProjectManagerId.Should().Be(pmId);
        project.DateRange.Should().Be(dateRange);
        project.Budget.Should().Be(budget);
        project.Priority.Should().Be(priority);
        project.Status.Should().Be(ProjectStatus.Planning);
        project.ProgressPercentage.Should().Be(0);
        project.CreatedBy.Should().Be("TestUser");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var code = "TEST-001";
        var orgId = Guid.NewGuid();
        var pmId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30));
        var budget = Money.Create(50000, "USD");

        // Act
        Action act = () => Project.Create(invalidName, code, orgId, pmId, dateRange, budget);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidCode_ShouldThrowArgumentException(string invalidCode)
    {
        // Arrange
        var name = "Test Project";
        var orgId = Guid.NewGuid();
        var pmId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30));
        var budget = Money.Create(50000, "USD");

        // Act
        Action act = () => Project.Create(name, invalidCode, orgId, pmId, dateRange, budget);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*code*");
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateProject()
    {
        // Arrange
        var project = CreateValidProject();
        var newName = "Updated Project Name";
        var newDescription = "Updated description";
        var newDateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(120));
        var newBudget = Money.Create(150000, "USD");
        var newPriority = ProjectPriority.Critical;

        // Act
        project.UpdateDetails(newName, newDescription, newDateRange, newBudget, newPriority, "UpdateUser");

        // Assert
        project.Name.Should().Be(newName);
        project.Description.Should().Be(newDescription);
        project.DateRange.Should().Be(newDateRange);
        project.Budget.Should().Be(newBudget);
        project.Priority.Should().Be(newPriority);
        project.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateProjectStatus()
    {
        // Arrange
        var project = CreateValidProject();
        var newStatus = ProjectStatus.Active;

        // Act
        project.ChangeStatus(newStatus, "UpdateUser");

        // Assert
        project.Status.Should().Be(newStatus);
        project.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void UpdateProgress_WithValidPercentage_ShouldUpdateProgress()
    {
        // Arrange
        var project = CreateValidProject();
        var progress = 45.5m;

        // Act
        project.UpdateProgress(progress, "UpdateUser");

        // Assert
        project.ProgressPercentage.Should().Be(progress);
        project.UpdatedBy.Should().Be("UpdateUser");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void UpdateProgress_WithInvalidPercentage_ShouldThrowArgumentException(decimal invalidProgress)
    {
        // Arrange
        var project = CreateValidProject();

        // Act
        Action act = () => project.UpdateProgress(invalidProgress, "UpdateUser");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // Helper method
    private static Project CreateValidProject()
    {
        return Project.Create(
            "Test Project",
            "TEST-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(60)),
            Money.Create(75000, "USD"),
            ProjectPriority.Medium,
            "Test Description",
            "TestUser"
        );
    }
}

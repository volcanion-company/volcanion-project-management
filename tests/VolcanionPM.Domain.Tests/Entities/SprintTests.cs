using FluentAssertions;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Domain.Tests.Entities;

public class SprintTests
{
    private static Sprint CreateValidSprint()
    {
        return Sprint.Create(
            "Sprint 1",
            1,
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(14)),
            "Complete user authentication features",
            30,
            "TestUser");
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateSprint()
    {
        // Arrange
        var name = "Sprint 1";
        var sprintNumber = 1;
        var projectId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(14));
        var goal = "Complete user authentication";
        var totalStoryPoints = 25;

        // Act
        var sprint = Sprint.Create(name, sprintNumber, projectId, dateRange, goal, totalStoryPoints, "TestUser");

        // Assert
        sprint.Should().NotBeNull();
        sprint.Name.Should().Be(name);
        sprint.SprintNumber.Should().Be(sprintNumber);
        sprint.ProjectId.Should().Be(projectId);
        sprint.DateRange.Should().Be(dateRange);
        sprint.Goal.Should().Be(goal);
        sprint.TotalStoryPoints.Should().Be(totalStoryPoints);
        sprint.Status.Should().Be(SprintStatus.Planned);
        sprint.CompletedStoryPoints.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange & Act
        var action = () => Sprint.Create(
            invalidName,
            1,
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(14)),
            "Goal",
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidSprintNumber_ShouldThrowArgumentException(int invalidNumber)
    {
        // Arrange & Act
        var action = () => Sprint.Create(
            "Sprint 1",
            invalidNumber,
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(14)),
            "Goal",
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*Sprint number*");
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateSprintDetails()
    {
        // Arrange
        var sprint = CreateValidSprint();
        var newName = "Sprint 1 - Updated";
        var newGoal = "Updated goal";
        var newDateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(15));
        var newStoryPoints = 35;

        // Act
        sprint.UpdateDetails(newName, newGoal, newDateRange, newStoryPoints, "UpdateUser");

        // Assert
        sprint.Name.Should().Be(newName);
        sprint.Goal.Should().Be(newGoal);
        sprint.DateRange.Should().Be(newDateRange);
        sprint.TotalStoryPoints.Should().Be(newStoryPoints);
        sprint.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void UpdateDetails_OnCompletedSprint_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var sprint = CreateValidSprint();
        // Start and complete the sprint
        sprint.GetType().GetProperty("Status")!.SetValue(sprint, SprintStatus.Completed);

        // Act
        var action = () => sprint.UpdateDetails(
            "Updated Name",
            "Updated Goal",
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(14)),
            40,
            "UpdateUser");

        // Assert
        action.Should().Throw<InvalidOperationException>().WithMessage("*completed sprint*");
    }

    [Fact]
    public void Start_OnPlannedSprint_ShouldChangeStatusToActive()
    {
        // Arrange
        var sprint = CreateValidSprint();

        // Act
        sprint.Start("StartUser");

        // Assert
        sprint.Status.Should().Be(SprintStatus.Active);
        sprint.UpdatedBy.Should().Be("StartUser");
    }

    [Fact]
    public void Start_OnNonPlannedSprint_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var sprint = CreateValidSprint();
        sprint.Start("StartUser"); // Already active

        // Act
        var action = () => sprint.Start("StartUser");

        // Assert
        action.Should().Throw<InvalidOperationException>().WithMessage("*Only planned sprints*");
    }

    [Fact]
    public void Start_BeforeStartDate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(10);
        var sprint = Sprint.Create(
            "Future Sprint",
            1,
            Guid.NewGuid(),
            DateRange.Create(futureDate, futureDate.AddDays(14)),
            null,
            null,
            "TestUser");

        // Act
        var action = () => sprint.Start("StartUser");

        // Assert
        action.Should().Throw<InvalidOperationException>().WithMessage("*before start date*");
    }

    [Fact]
    public void Complete_OnActiveSprint_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var sprint = CreateValidSprint();
        sprint.Start("StartUser");

        // Act
        sprint.Complete("CompleteUser");

        // Assert
        sprint.Status.Should().Be(SprintStatus.Completed);
        sprint.UpdatedBy.Should().Be("CompleteUser");
    }

    [Fact]
    public void Complete_OnNonActiveSprint_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var sprint = CreateValidSprint(); // Planned status

        // Act
        var action = () => sprint.Complete("CompleteUser");

        // Assert
        action.Should().Throw<InvalidOperationException>().WithMessage("*Only active sprints*");
    }

    [Fact]
    public void Cancel_WithReason_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var sprint = CreateValidSprint();
        var reason = "Requirements changed";

        // Act
        sprint.Cancel(reason, "CancelUser");

        // Assert
        sprint.Status.Should().Be(SprintStatus.Cancelled);
        sprint.UpdatedBy.Should().Be("CancelUser");
    }

    [Fact]
    public void Cancel_OnCompletedSprint_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var sprint = CreateValidSprint();
        // Force completed status
        sprint.GetType().GetProperty("Status")!.SetValue(sprint, SprintStatus.Completed);

        // Act
        var action = () => sprint.Cancel("Reason", "CancelUser");

        // Assert
        action.Should().Throw<InvalidOperationException>().WithMessage("*Cannot cancel completed sprint*");
    }

    [Fact]
    public void GetCompletedTaskCount_ShouldReturnZeroForNewSprint()
    {
        // Arrange
        var sprint = CreateValidSprint();

        // Act
        var count = sprint.GetCompletedTaskCount();

        // Assert
        count.Should().Be(0);
    }
}

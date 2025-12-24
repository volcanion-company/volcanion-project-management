using FluentAssertions;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;

namespace VolcanionPM.Domain.Tests.Entities;

public class ProjectTaskTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var title = "Implement Login Feature";
        var code = "TASK-001";
        var projectId = Guid.NewGuid();
        var type = TaskType.Feature;
        var priority = TaskPriority.High;
        var estimatedHours = 8m;

        // Act
        var task = ProjectTask.Create(
            title,
            code,
            projectId,
            type,
            priority,
            estimatedHours,
            "Create login functionality",
            null,
            null,
            null,
            null,
            5,
            "TestUser"
        );

        // Assert
        task.Should().NotBeNull();
        task.Id.Should().NotBeEmpty();
        task.Title.Should().Be(title);
        task.Code.Should().Be(code.ToUpperInvariant());
        task.ProjectId.Should().Be(projectId);
        task.Type.Should().Be(type);
        task.Priority.Should().Be(priority);
        task.Status.Should().Be(TaskStatus.Backlog);
        task.EstimatedHours.Should().Be(estimatedHours);
        task.ActualHours.Should().Be(0);
        task.StoryPoints.Should().Be(5);
        task.CreatedBy.Should().Be("TestUser");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Act
        Action act = () => ProjectTask.Create(
            invalidTitle,
            "TASK-001",
            Guid.NewGuid(),
            TaskType.Task,
            TaskPriority.Medium,
            8m,
            "Description",
            null,
            null,
            null,
            null,
            null,
            "TestUser"
        );

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*title*");
    }

    [Fact]
    public void Create_WithNegativeEstimatedHours_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => ProjectTask.Create(
            "Test Task",
            "TASK-001",
            Guid.NewGuid(),
            TaskType.Task,
            TaskPriority.Medium,
            -5m,
            "Description"
        );

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*estimated hours*");
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateTask()
    {
        // Arrange
        var task = CreateValidTask();
        var newTitle = "Updated Task Title";
        var newDescription = "Updated Description";
        var newPriority = TaskPriority.Low;
        var newEstimatedHours = 12m;

        // Act
        task.UpdateDetails(newTitle, newDescription, TaskType.Bug, newPriority, newEstimatedHours, DateTime.UtcNow.AddDays(7), 8, "UpdateUser");

        // Assert
        task.Title.Should().Be(newTitle);
        task.Description.Should().Be(newDescription);
        task.Type.Should().Be(TaskType.Bug);
        task.Priority.Should().Be(newPriority);
        task.EstimatedHours.Should().Be(newEstimatedHours);
        task.StoryPoints.Should().Be(8);
        task.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void ChangeStatus_WithValidStatus_ShouldUpdateStatus()
    {
        // Arrange
        var task = CreateValidTask();
        var newStatus = TaskStatus.InProgress;

        // Act
        task.ChangeStatus(newStatus, "UpdateUser");

        // Assert
        task.Status.Should().Be(newStatus);
        task.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void AssignTo_WithUserId_ShouldAssignTask()
    {
        // Arrange
        var task = CreateValidTask();
        var userId = Guid.NewGuid();

        // Act
        task.AssignTo(userId, "AssignUser");

        // Assert
        task.AssignedToId.Should().Be(userId);
        task.UpdatedBy.Should().Be("AssignUser");
    }

    [Fact]
    public void Unassign_ShouldRemoveAssignment()
    {
        // Arrange
        var task = CreateValidTask();
        task.AssignTo(Guid.NewGuid(), "TestUser");

        // Act
        task.Unassign("UnassignUser");

        // Assert
        task.AssignedToId.Should().BeNull();
        task.UpdatedBy.Should().Be("UnassignUser");
    }

    [Fact]
    public void RecordTime_WithValidHours_ShouldUpdateActualHours()
    {
        // Arrange
        var task = CreateValidTask();
        var additionalHours = 3.5m;

        // Act
        task.RecordTime(additionalHours);

        // Assert
        task.ActualHours.Should().Be(additionalHours);
    }

    [Fact]
    public void RecordTime_MultipleTimes_ShouldAccumulateHours()
    {
        // Arrange
        var task = CreateValidTask();

        // Act
        task.RecordTime(2.5m);
        task.RecordTime(3.0m);
        task.RecordTime(1.5m);

        // Assert
        task.ActualHours.Should().Be(7.0m);
    }

    // Helper method
    private static ProjectTask CreateValidTask()
    {
        return ProjectTask.Create(
            "Test Task",
            "TASK-001",
            Guid.NewGuid(),
            TaskType.Task,
            TaskPriority.Medium,
            8m,
            "Test Description",
            null,
            null,
            null,
            null,
            null,
            "TestUser"
        );
    }
}

using FluentAssertions;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Domain.Tests.Entities;

public class TimeEntryTests
{
    private static TimeEntry CreateValidTimeEntry()
    {
        return TimeEntry.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            4.5m,
            TimeEntryType.Development,
            DateTime.UtcNow.Date,
            "Working on feature implementation",
            true,
            "TestUser");
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateTimeEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var hours = 6.5m;
        var type = TimeEntryType.Development;
        var date = DateTime.UtcNow.Date;
        var description = "Working on bug fixes";
        var isBillable = true;

        // Act
        var timeEntry = TimeEntry.Create(userId, taskId, hours, type, date, description, isBillable, "TestUser");

        // Assert
        timeEntry.Should().NotBeNull();
        timeEntry.UserId.Should().Be(userId);
        timeEntry.TaskId.Should().Be(taskId);
        timeEntry.Hours.Should().Be(hours);
        timeEntry.Type.Should().Be(type);
        timeEntry.Date.Should().Be(date);
        timeEntry.Description.Should().Be(description);
        timeEntry.IsBillable.Should().Be(isBillable);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5.5)]
    public void Create_WithNonPositiveHours_ShouldThrowArgumentException(decimal invalidHours)
    {
        // Arrange & Act
        var action = () => TimeEntry.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            invalidHours,
            TimeEntryType.Development,
            DateTime.UtcNow.Date,
            null,
            true,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*Hours must be positive*");
    }

    [Theory]
    [InlineData(24.1)]
    [InlineData(25)]
    [InlineData(100)]
    public void Create_WithMoreThan24Hours_ShouldThrowArgumentException(decimal invalidHours)
    {
        // Arrange & Act
        var action = () => TimeEntry.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            invalidHours,
            TimeEntryType.Development,
            DateTime.UtcNow.Date,
            null,
            true,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*Cannot log more than 24 hours*");
    }

    [Fact]
    public void Create_WithMaximumValidHours_ShouldSucceed()
    {
        // Arrange & Act
        var timeEntry = TimeEntry.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            24m,
            TimeEntryType.Development,
            DateTime.UtcNow.Date,
            null,
            true,
            "TestUser");

        // Assert
        timeEntry.Hours.Should().Be(24m);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateTimeEntry()
    {
        // Arrange
        var timeEntry = CreateValidTimeEntry();
        var newHours = 8m;
        var newType = TimeEntryType.Testing;
        var newDate = DateTime.UtcNow.Date.AddDays(-1);
        var newDescription = "Updated description";
        var newBillable = false;

        // Act
        timeEntry.Update(newHours, newType, newDate, newDescription, newBillable, "UpdateUser");

        // Assert
        timeEntry.Hours.Should().Be(newHours);
        timeEntry.Type.Should().Be(newType);
        timeEntry.Date.Should().Be(newDate);
        timeEntry.Description.Should().Be(newDescription);
        timeEntry.IsBillable.Should().Be(newBillable);
        timeEntry.UpdatedBy.Should().Be("UpdateUser");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_WithInvalidHours_ShouldThrowArgumentException(decimal invalidHours)
    {
        // Arrange
        var timeEntry = CreateValidTimeEntry();

        // Act
        var action = () => timeEntry.Update(
            invalidHours,
            TimeEntryType.Development,
            DateTime.UtcNow.Date,
            null,
            true,
            "UpdateUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*Hours must be positive*");
    }

    [Fact]
    public void Create_WithDifferentTimeEntryTypes_ShouldSucceed()
    {
        // Arrange & Act
        var developmentEntry = TimeEntry.Create(Guid.NewGuid(), Guid.NewGuid(), 8m, TimeEntryType.Development, DateTime.UtcNow.Date);
        var testingEntry = TimeEntry.Create(Guid.NewGuid(), Guid.NewGuid(), 4m, TimeEntryType.Testing, DateTime.UtcNow.Date);

        // Assert
        developmentEntry.Type.Should().Be(TimeEntryType.Development);
        testingEntry.Type.Should().Be(TimeEntryType.Testing);
    }

    [Fact]
    public void Create_ShouldNormalizeDateToDateOnly()
    {
        // Arrange
        var dateWithTime = new DateTime(2024, 12, 15, 14, 30, 45);

        // Act
        var timeEntry = TimeEntry.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5m,
            TimeEntryType.Development,
            dateWithTime);

        // Assert
        timeEntry.Date.Should().Be(new DateTime(2024, 12, 15)); // Time component removed
        timeEntry.Date.Hour.Should().Be(0);
        timeEntry.Date.Minute.Should().Be(0);
        timeEntry.Date.Second.Should().Be(0);
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSucceed()
    {
        // Arrange & Act
        var timeEntry = TimeEntry.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            4m,
            TimeEntryType.Development,
            DateTime.UtcNow.Date,
            null,
            true,
            "TestUser");

        // Assert
        timeEntry.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithWhitespaceDescription_ShouldTrimDescription()
    {
        // Arrange
        var description = "  Test description  ";

        // Act
        var timeEntry = TimeEntry.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            4m,
            TimeEntryType.Development,
            DateTime.UtcNow.Date,
            description,
            true,
            "TestUser");

        // Assert
        timeEntry.Description.Should().Be("Test description");
    }
}

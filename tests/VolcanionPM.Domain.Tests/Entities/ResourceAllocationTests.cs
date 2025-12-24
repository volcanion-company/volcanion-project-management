using FluentAssertions;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Domain.Tests.Entities;

public class ResourceAllocationTests
{
    private static ResourceAllocation CreateValidAllocation()
    {
        return ResourceAllocation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30)),
            ResourceAllocationType.FullTime,
            100m,
            Money.Create(75, "USD"),
            "Full-time developer allocation",
            "TestUser");
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateResourceAllocation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var allocationPeriod = DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(60));
        var type = ResourceAllocationType.PartTime;
        var percentage = 50m;
        var hourlyRate = Money.Create(80, "USD");
        var notes = "Part-time consultant";

        // Act
        var allocation = ResourceAllocation.Create(userId, projectId, allocationPeriod, 
            type, percentage, hourlyRate, notes, "TestUser");

        // Assert
        allocation.Should().NotBeNull();
        allocation.UserId.Should().Be(userId);
        allocation.ProjectId.Should().Be(projectId);
        allocation.AllocationPeriod.Should().Be(allocationPeriod);
        allocation.Type.Should().Be(type);
        allocation.AllocationPercentage.Should().Be(percentage);
        allocation.HourlyRate.Should().Be(hourlyRate);
        allocation.Notes.Should().Be(notes);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(-50)]
    [InlineData(150)]
    public void Create_WithInvalidAllocationPercentage_ShouldThrowArgumentException(decimal invalidPercentage)
    {
        // Arrange & Act
        var action = () => ResourceAllocation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30)),
            ResourceAllocationType.FullTime,
            invalidPercentage,
            null,
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Allocation percentage must be between 0 and 100*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public void Create_WithValidAllocationPercentages_ShouldSucceed(decimal percentage)
    {
        // Arrange & Act
        var allocation = ResourceAllocation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30)),
            ResourceAllocationType.PartTime,
            percentage);

        // Assert
        allocation.AllocationPercentage.Should().Be(percentage);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllocation()
    {
        // Arrange
        var allocation = CreateValidAllocation();
        var newPeriod = DateRange.Create(DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(40));
        var newType = ResourceAllocationType.PartTime;
        var newPercentage = 75m;
        var newRate = Money.Create(90, "USD");
        var newNotes = "Updated allocation";

        // Act
        allocation.Update(newPeriod, newType, newPercentage, newRate, newNotes, "UpdateUser");

        // Assert
        allocation.AllocationPeriod.Should().Be(newPeriod);
        allocation.Type.Should().Be(newType);
        allocation.AllocationPercentage.Should().Be(newPercentage);
        allocation.HourlyRate.Should().Be(newRate);
        allocation.Notes.Should().Be(newNotes);
        allocation.UpdatedBy.Should().Be("UpdateUser");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Update_WithInvalidPercentage_ShouldThrowArgumentException(decimal invalidPercentage)
    {
        // Arrange
        var allocation = CreateValidAllocation();

        // Act
        var action = () => allocation.Update(
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30)),
            ResourceAllocationType.FullTime,
            invalidPercentage,
            null,
            null,
            "UpdateUser");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Allocation percentage must be between 0 and 100*");
    }

    [Fact]
    public void IsActiveOn_WithDateInRange_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date;
        var endDate = DateTime.UtcNow.Date.AddDays(30);
        var allocation = ResourceAllocation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(startDate, endDate),
            ResourceAllocationType.FullTime,
            100m);

        // Act
        var isActive = allocation.IsActiveOn(startDate.AddDays(15));

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void IsActiveOn_WithDateOutsideRange_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date.AddDays(-60);
        var endDate = DateTime.UtcNow.Date.AddDays(-30);
        var allocation = ResourceAllocation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(startDate, endDate),
            ResourceAllocationType.FullTime,
            100m);

        // Act
        var isActive = allocation.IsActiveOn(DateTime.UtcNow.Date);

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentlyActive_WithCurrentDate_ShouldReturnTrue()
    {
        // Arrange
        var allocation = ResourceAllocation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date.AddDays(-10), DateTime.UtcNow.Date.AddDays(10)),
            ResourceAllocationType.FullTime,
            100m);

        // Act
        var isActive = allocation.IsCurrentlyActive();

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentlyActive_WithPastDate_ShouldReturnFalse()
    {
        // Arrange
        var allocation = ResourceAllocation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date.AddDays(-30), DateTime.UtcNow.Date.AddDays(-10)),
            ResourceAllocationType.FullTime,
            100m);

        // Act
        var isActive = allocation.IsCurrentlyActive();

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNullOptionalParameters_ShouldSucceed()
    {
        // Arrange & Act
        var allocation = ResourceAllocation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30)),
            ResourceAllocationType.Contract,
            50m,
            null, // hourlyRate
            null, // notes
            "TestUser");

        // Assert
        allocation.HourlyRate.Should().BeNull();
        allocation.Notes.Should().BeNull();
    }

    [Fact]
    public void Create_WithDifferentAllocationType_ShouldSetCorrectType()
    {
        // Arrange & Act
        var fullTime = ResourceAllocation.Create(Guid.NewGuid(), Guid.NewGuid(), 
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30)),
            ResourceAllocationType.FullTime, 100m);
        
        var partTime = ResourceAllocation.Create(Guid.NewGuid(), Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30)),
            ResourceAllocationType.PartTime, 50m);
        
        var contract = ResourceAllocation.Create(Guid.NewGuid(), Guid.NewGuid(),
            DateRange.Create(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30)),
            ResourceAllocationType.Contract, 75m);

        // Assert
        fullTime.Type.Should().Be(ResourceAllocationType.FullTime);
        partTime.Type.Should().Be(ResourceAllocationType.PartTime);
        contract.Type.Should().Be(ResourceAllocationType.Contract);
    }
}

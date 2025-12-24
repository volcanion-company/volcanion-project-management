using FluentAssertions;
using VolcanionPM.Application.Features.Projects.Commands.Create;

namespace VolcanionPM.Application.Tests.Features.Projects.Validators;

public class CreateProjectCommandValidatorTests
{
    private readonly CreateProjectCommandValidator _validator;

    public CreateProjectCommandValidatorTests()
    {
        _validator = new CreateProjectCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
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

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidName_ShouldFail(string invalidName)
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = invalidName!,
            Code = "TEST-001",
            Priority = "Medium",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithTooLongName_ShouldFail()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = new string('A', 201), // 201 characters
            Code = "TEST-001",
            Priority = "Medium",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("200"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidCode_ShouldFail(string invalidCode)
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = invalidCode!,
            Priority = "Medium",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code");
    }

    [Theory]
    [InlineData("test-001")] // lowercase
    [InlineData("TEST_001")] // underscore
    [InlineData("TEST 001")] // space
    [InlineData("TEST@001")] // special character
    public void Validate_WithInvalidCodeFormat_ShouldFail(string invalidCode)
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = invalidCode,
            Priority = "Medium",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("uppercase"));
    }

    [Fact]
    public void Validate_WithTooLongCode_ShouldFail()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = new string('A', 21), // 21 characters
            Priority = "Medium",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("20"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidPriority_ShouldFail(string invalidPriority)
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = invalidPriority!,
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("VeryHigh")]
    [InlineData("Urgent")]
    public void Validate_WithInvalidPriorityValue_ShouldFail(string invalidPriority)
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = invalidPriority,
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority" && e.ErrorMessage.Contains("Low, Medium, High, or Critical"));
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    [InlineData("Critical")]
    public void Validate_WithValidPriorityValues_ShouldPass(string validPriority)
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = validPriority,
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithTooLongDescription_ShouldFail()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Description = new string('A', 2001), // 2001 characters
            Priority = "Medium",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("2000"));
    }

    [Fact]
    public void Validate_WithEmptyOrganizationId_ShouldFail()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = "Medium",
            OrganizationId = Guid.Empty,
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrganizationId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidCreatedBy_ShouldFail(string invalidCreatedBy)
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = "Medium",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = invalidCreatedBy!
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CreatedBy");
    }

    [Fact]
    public void Validate_WithEndDateBeforeStartDate_ShouldFail()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = "Medium",
            StartDate = DateTime.UtcNow.Date.AddDays(30),
            EndDate = DateTime.UtcNow.Date, // Before start date
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndDate" && e.ErrorMessage.Contains("after start date"));
    }

    [Fact]
    public void Validate_WithNegativeBudget_ShouldFail()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = "Medium",
            BudgetAmount = -1000m,
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BudgetAmount" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_WithZeroBudget_ShouldFail()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = "Medium",
            BudgetAmount = 0m,
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BudgetAmount");
    }

    [Fact]
    public void Validate_WithNullBudget_ShouldPass()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = "Medium",
            BudgetAmount = null,
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullDescription_ShouldPass()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Code = "TEST-001",
            Priority = "Medium",
            Description = null,
            OrganizationId = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

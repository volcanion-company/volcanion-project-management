using FluentAssertions;
using VolcanionPM.Application.Features.Users.Commands.Create;
using VolcanionPM.Domain.Enums;
using Xunit;

namespace VolcanionPM.Application.Tests.Features.Users.Validators;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        _validator = new CreateUserCommandValidator();
    }

    #region FirstName Validation

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidFirstName_ShouldFail(string? invalidFirstName)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = invalidFirstName!,
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.FirstName));
    }

    [Fact]
    public void Validate_WithTooLongFirstName_ShouldFail()
    {
        // Arrange - 101 characters
        var tooLongName = new string('A', 101);
        var command = new CreateUserCommand
        {
            FirstName = tooLongName,
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateUserCommand.FirstName) && 
            e.ErrorMessage.Contains("100 characters"));
    }

    #endregion

    #region LastName Validation

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidLastName_ShouldFail(string? invalidLastName)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = invalidLastName!,
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.LastName));
    }

    [Fact]
    public void Validate_WithTooLongLastName_ShouldFail()
    {
        // Arrange - 101 characters
        var tooLongName = new string('B', 101);
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = tooLongName,
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateUserCommand.LastName) && 
            e.ErrorMessage.Contains("100 characters"));
    }

    #endregion

    #region Email Validation

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidEmail_ShouldFail(string? invalidEmail)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = invalidEmail!,
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain.com")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string invalidEmail)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = invalidEmail,
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateUserCommand.Email) && 
            e.ErrorMessage.Contains("email"));
    }

    [Fact]
    public void Validate_WithValidEmail_ShouldPass()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "valid@email.com",
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Password Validation

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyPassword_ShouldFail(string? invalidPassword)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = invalidPassword!,
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Password));
    }

    [Fact]
    public void Validate_WithShortPassword_ShouldFail()
    {
        // Arrange - Only 7 characters
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Short1!",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateUserCommand.Password) && 
            e.ErrorMessage.Contains("8 characters"));
    }

    [Fact]
    public void Validate_WithPasswordMissingUppercase_ShouldFail()
    {
        // Arrange - No uppercase letter
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "password123!",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateUserCommand.Password) && 
            e.ErrorMessage.Contains("uppercase"));
    }

    [Fact]
    public void Validate_WithPasswordMissingLowercase_ShouldFail()
    {
        // Arrange - No lowercase letter
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "PASSWORD123!",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateUserCommand.Password) && 
            e.ErrorMessage.Contains("lowercase"));
    }

    [Fact]
    public void Validate_WithPasswordMissingNumber_ShouldFail()
    {
        // Arrange - No number
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password!",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateUserCommand.Password) && 
            e.ErrorMessage.Contains("number"));
    }

    [Fact]
    public void Validate_WithPasswordMissingSpecialCharacter_ShouldFail()
    {
        // Arrange - No special character
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateUserCommand.Password) && 
            e.ErrorMessage.Contains("special character"));
    }

    #endregion

    #region Role Validation

    [Fact]
    public void Validate_WithInvalidRole_ShouldFail()
    {
        // Arrange - Using an invalid enum value (99)
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            Role = (UserRole)99,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Role));
    }

    [Theory]
    [InlineData(UserRole.Developer)]
    [InlineData(UserRole.ProjectManager)]
    [InlineData(UserRole.TeamLead)]
    [InlineData(UserRole.Designer)]
    [InlineData(UserRole.Tester)]
    public void Validate_WithValidRoles_ShouldPass(UserRole validRole)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            Role = validRole,
            OrganizationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region OrganizationId Validation

    [Fact]
    public void Validate_WithEmptyOrganizationId_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.OrganizationId));
    }

    #endregion

    #region Valid Command

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid(),
            PhoneNumber = "+1234567890"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithValidCommandAndNullPhone_ShouldPass()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            Role = UserRole.Developer,
            OrganizationId = Guid.NewGuid(),
            PhoneNumber = null
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion
}

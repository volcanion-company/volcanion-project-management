using FluentAssertions;
using VolcanionPM.Application.Features.Auth.Commands.Login;
using Xunit;

namespace VolcanionPM.Application.Tests.Features.Auth.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    #region Email Validation

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidEmail_ShouldFail(string? invalidEmail)
    {
        // Arrange
        var command = new LoginCommand(invalidEmail!, "password123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain.com")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string invalidEmail)
    {
        // Arrange
        var command = new LoginCommand(invalidEmail, "password123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(LoginCommand.Email) && 
            e.ErrorMessage.Contains("email"));
    }

    [Fact]
    public void Validate_WithValidEmail_ShouldPass()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "password123");

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
    public void Validate_WithInvalidPassword_ShouldFail(string? invalidPassword)
    {
        // Arrange
        var command = new LoginCommand("user@example.com", invalidPassword!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Fact]
    public void Validate_WithValidPassword_ShouldPass()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "anypassword");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Valid Command

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "SecurePassword123!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion
}

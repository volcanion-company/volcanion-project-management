using FluentAssertions;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var passwordHash = "hashedPassword123";
        var orgId = Guid.NewGuid();
        var role = UserRole.Developer;

        // Act
        var user = User.Create(firstName, lastName, email, passwordHash, orgId, role, "+1234567890", "TestUser");

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Value.Should().Be(email.ToLowerInvariant());
        user.PasswordHash.Should().Be(passwordHash);
        user.OrganizationId.Should().Be(orgId);
        user.Role.Should().Be(role);
        user.PhoneNumber.Should().Be("+1234567890");
        user.IsActive.Should().BeTrue();
        user.EmailConfirmed.Should().BeFalse();
        user.CreatedBy.Should().Be("TestUser");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidFirstName_ShouldThrowArgumentException(string invalidFirstName)
    {
        // Act
        Action act = () => User.Create(invalidFirstName, "Doe", "john@example.com", "hash", Guid.NewGuid(), UserRole.Developer);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*first name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidLastName_ShouldThrowArgumentException(string invalidLastName)
    {
        // Act
        Action act = () => User.Create("John", invalidLastName, "john@example.com", "hash", Guid.NewGuid(), UserRole.Developer);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*last name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act
        Action act = () => User.Create("John", "Doe", invalidEmail, "hash", Guid.NewGuid(), UserRole.Developer);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var user = CreateValidUser();
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newPhone = "+9876543210";

        // Act
        user.UpdateProfile(newFirstName, newLastName, newPhone, "UpdateUser");

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.PhoneNumber.Should().Be(newPhone);
        user.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void ChangePassword_WithValidHash_ShouldUpdatePasswordHash()
    {
        // Arrange
        var user = CreateValidUser();
        var newHash = "newHashedPassword456";

        // Act
        user.ChangePassword(newHash, "ChangeUser");

        // Assert
        user.PasswordHash.Should().Be(newHash);
        user.UpdatedBy.Should().Be("ChangeUser");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ChangePassword_WithInvalidHash_ShouldThrowArgumentException(string invalidHash)
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        Action act = () => user.ChangePassword(invalidHash, "ChangeUser");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = CreateValidUser();
        user.Deactivate("TestUser");

        // Act
        user.Activate("ActivateUser");

        // Assert
        user.IsActive.Should().BeTrue();
        user.UpdatedBy.Should().Be("ActivateUser");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.Deactivate("DeactivateUser");

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedBy.Should().Be("DeactivateUser");
    }

    [Fact]
    public void GetFullName_ShouldReturnCombinedName()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var fullName = user.GetFullName();

        // Assert
        fullName.Should().Be("John Doe");
    }

    // Helper method
    private static User CreateValidUser()
    {
        return User.Create(
            "John",
            "Doe",
            "john.doe@example.com",
            "hashedPassword123",
            Guid.NewGuid(),
            UserRole.Developer,
            "+1234567890",
            "TestUser"
        );
    }
}

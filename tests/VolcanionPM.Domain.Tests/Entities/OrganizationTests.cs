using FluentAssertions;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Domain.Tests.Entities;

public class OrganizationTests
{
    private static Organization CreateValidOrganization()
    {
        return Organization.Create(
            "Acme Corporation",
            "Leading software development company",
            "https://acme.com",
            Address.Create("123 Main St", "Tech City", "CA", "USA", "12345"),
            "TestUser");
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateOrganization()
    {
        // Arrange
        var name = "TechStart Inc.";
        var description = "Innovative startup company";
        var website = "https://techstart.io";
        var address = Address.Create("456 Innovation Blvd", "Silicon Valley", "CA", "USA", "94000");

        // Act
        var organization = Organization.Create(name, description, website, address, "TestUser");

        // Assert
        organization.Should().NotBeNull();
        organization.Name.Should().Be(name);
        organization.Description.Should().Be(description);
        organization.Website.Should().Be(website);
        organization.Address.Should().Be(address);
        organization.IsActive.Should().BeTrue();
        organization.SubscriptionExpiryDate.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange & Act
        var action = () => Organization.Create(
            invalidName,
            "Description",
            null,
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateOrganization()
    {
        // Arrange
        var organization = CreateValidOrganization();
        var newName = "Acme Corporation Ltd.";
        var newDescription = "Updated description";
        var newWebsite = "https://acme-corp.com";
        var newAddress = Address.Create("789 New St", "New City", "NY", "USA", "10001");

        // Act
        organization.UpdateDetails(newName, newDescription, newWebsite, newAddress, "UpdateUser");

        // Assert
        organization.Name.Should().Be(newName);
        organization.Description.Should().Be(newDescription);
        organization.Website.Should().Be(newWebsite);
        organization.Address.Should().Be(newAddress);
        organization.UpdatedBy.Should().Be("UpdateUser");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateDetails_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var organization = CreateValidOrganization();

        // Act
        var action = () => organization.UpdateDetails(
            invalidName,
            "Description",
            null,
            null,
            "UpdateUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Fact]
    public void SetLogo_ShouldUpdateLogoUrl()
    {
        // Arrange
        var organization = CreateValidOrganization();
        var logoUrl = "https://cdn.acme.com/logo.png";

        // Act
        organization.SetLogo(logoUrl, "UpdateUser");

        // Assert
        organization.LogoUrl.Should().Be(logoUrl);
        organization.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var organization = CreateValidOrganization();

        // Act
        organization.Deactivate("DeactivateUser");

        // Assert
        organization.IsActive.Should().BeFalse();
        organization.UpdatedBy.Should().Be("DeactivateUser");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var organization = CreateValidOrganization();
        organization.Deactivate("User");

        // Act
        organization.Activate("ActivateUser");

        // Assert
        organization.IsActive.Should().BeTrue();
        organization.UpdatedBy.Should().Be("ActivateUser");
    }

    [Fact]
    public void SetSubscriptionExpiry_ShouldUpdateExpiryDate()
    {
        // Arrange
        var organization = CreateValidOrganization();
        var expiryDate = DateTime.UtcNow.Date.AddYears(1);

        // Act
        organization.SetSubscriptionExpiry(expiryDate, "UpdateUser");

        // Assert
        organization.SubscriptionExpiryDate.Should().Be(expiryDate);
        organization.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void Create_WithNullOptionalParameters_ShouldSucceed()
    {
        // Arrange & Act
        var organization = Organization.Create(
            "Simple Org",
            null, // description
            null, // website
            null, // address
            "TestUser");

        // Assert
        organization.Name.Should().Be("Simple Org");
        organization.Description.Should().BeNull();
        organization.Website.Should().BeNull();
        organization.Address.Should().BeNull();
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldTrimName()
    {
        // Arrange
        var name = "  Test Organization  ";

        // Act
        var organization = Organization.Create(name);

        // Assert
        organization.Name.Should().Be("Test Organization");
    }

    [Fact]
    public void Create_ShouldInitializeEmptyCollections()
    {
        // Arrange & Act
        var organization = Organization.Create("Test Org");

        // Assert
        organization.Users.Should().BeEmpty();
        organization.Projects.Should().BeEmpty();
    }

    [Fact]
    public void Create_NewOrganization_ShouldBeActive()
    {
        // Arrange & Act
        var organization = Organization.Create("Active Org");

        // Assert
        organization.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ThenActivate_ShouldRestoreActiveState()
    {
        // Arrange
        var organization = CreateValidOrganization();

        // Act
        organization.Deactivate("User1");
        organization.IsActive.Should().BeFalse();

        organization.Activate("User2");

        // Assert
        organization.IsActive.Should().BeTrue();
    }
}

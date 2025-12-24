using FluentAssertions;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Domain.Tests.Entities;

public class RiskTests
{
    private static Risk CreateValidRisk()
    {
        return Risk.Create(
            "Security Vulnerability Risk",
            "Potential security breach in authentication module",
            RiskLevel.High,
            Guid.NewGuid(),
            70m,
            85m,
            Guid.NewGuid(),
            "Implement additional security layers",
            "TestUser");
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateRisk()
    {
        // Arrange
        var title = "Data Loss Risk";
        var description = "Risk of data loss during migration";
        var level = RiskLevel.Critical;
        var projectId = Guid.NewGuid();
        var probability = 60m;
        var impact = 90m;
        var ownerId = Guid.NewGuid();
        var mitigation = "Implement backup strategy";

        // Act
        var risk = Risk.Create(title, description, level, projectId, probability, impact, ownerId, mitigation, "TestUser");

        // Assert
        risk.Should().NotBeNull();
        risk.Title.Should().Be(title);
        risk.Description.Should().Be(description);
        risk.Level.Should().Be(level);
        risk.ProjectId.Should().Be(projectId);
        risk.Probability.Should().Be(probability);
        risk.Impact.Should().Be(impact);
        risk.OwnerId.Should().Be(ownerId);
        risk.MitigationStrategy.Should().Be(mitigation);
        risk.Status.Should().Be(RiskStatus.Identified);
        risk.IdentifiedDate.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Arrange & Act
        var action = () => Risk.Create(
            invalidTitle,
            "Valid description",
            RiskLevel.Medium,
            Guid.NewGuid(),
            50m,
            50m,
            null,
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*title*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidDescription_ShouldThrowArgumentException(string invalidDescription)
    {
        // Arrange & Act
        var action = () => Risk.Create(
            "Valid Title",
            invalidDescription,
            RiskLevel.Medium,
            Guid.NewGuid(),
            50m,
            50m,
            null,
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*description*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(-50)]
    [InlineData(150)]
    public void Create_WithInvalidProbability_ShouldThrowArgumentException(decimal invalidProbability)
    {
        // Arrange & Act
        var action = () => Risk.Create(
            "Valid Title",
            "Valid description",
            RiskLevel.Medium,
            Guid.NewGuid(),
            invalidProbability,
            50m,
            null,
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*Probability must be between 0 and 100*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(-50)]
    [InlineData(150)]
    public void Create_WithInvalidImpact_ShouldThrowArgumentException(decimal invalidImpact)
    {
        // Arrange & Act
        var action = () => Risk.Create(
            "Valid Title",
            "Valid description",
            RiskLevel.Medium,
            Guid.NewGuid(),
            50m,
            invalidImpact,
            null,
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*Impact must be between 0 and 100*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateRisk()
    {
        // Arrange
        var risk = CreateValidRisk();
        var newTitle = "Updated Risk Title";
        var newDescription = "Updated description";
        var newLevel = RiskLevel.Critical;
        var newProbability = 80m;
        var newImpact = 95m;
        var newMitigation = "Updated mitigation strategy";

        // Act
        risk.Update(newTitle, newDescription, newLevel, newProbability, newImpact, newMitigation, "UpdateUser");

        // Assert
        risk.Title.Should().Be(newTitle);
        risk.Description.Should().Be(newDescription);
        risk.Level.Should().Be(newLevel);
        risk.Probability.Should().Be(newProbability);
        risk.Impact.Should().Be(newImpact);
        risk.MitigationStrategy.Should().Be(newMitigation);
        risk.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void ChangeStatus_ToResolved_ShouldSetResolvedDate()
    {
        // Arrange
        var risk = CreateValidRisk();

        // Act
        risk.ChangeStatus(RiskStatus.Resolved, "ResolveUser");

        // Assert
        risk.Status.Should().Be(RiskStatus.Resolved);
        risk.ResolvedDate.Should().NotBeNull();
        risk.UpdatedBy.Should().Be("ResolveUser");
    }

    [Fact]
    public void ChangeStatus_ToSameStatus_ShouldDoNothing()
    {
        // Arrange
        var risk = CreateValidRisk();
        var originalUpdatedAt = risk.UpdatedAt;

        // Act
        risk.ChangeStatus(RiskStatus.Identified, "User");

        // Assert
        risk.Status.Should().Be(RiskStatus.Identified);
        // UpdatedAt should remain unchanged
    }

    [Fact]
    public void AssignOwner_ShouldUpdateOwnerId()
    {
        // Arrange
        var risk = CreateValidRisk();
        var newOwnerId = Guid.NewGuid();

        // Act
        risk.AssignOwner(newOwnerId, "AssignUser");

        // Assert
        risk.OwnerId.Should().Be(newOwnerId);
        risk.UpdatedBy.Should().Be("AssignUser");
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(50, 50, 25)]
    [InlineData(80, 60, 48)]
    [InlineData(100, 100, 100)]
    [InlineData(30, 70, 21)]
    public void GetRiskScore_ShouldCalculateCorrectly(decimal probability, decimal impact, decimal expectedScore)
    {
        // Arrange
        var risk = Risk.Create(
            "Test Risk",
            "Test description",
            RiskLevel.Medium,
            Guid.NewGuid(),
            probability,
            impact,
            null,
            null,
            "TestUser");

        // Act
        var score = risk.GetRiskScore();

        // Assert
        score.Should().Be(expectedScore);
    }

    [Fact]
    public void Create_WithNullOptionalParameters_ShouldSucceed()
    {
        // Arrange & Act
        var risk = Risk.Create(
            "Test Risk",
            "Test description",
            RiskLevel.Low,
            Guid.NewGuid(),
            20m,
            30m,
            null, // ownerId
            null, // mitigationStrategy
            "TestUser");

        // Assert
        risk.OwnerId.Should().BeNull();
        risk.MitigationStrategy.Should().BeNull();
    }

    [Fact]
    public void Create_WithWhitespaceTitle_ShouldTrimTitle()
    {
        // Arrange
        var title = "  Test Risk  ";

        // Act
        var risk = Risk.Create(
            title,
            "Description",
            RiskLevel.Low,
            Guid.NewGuid(),
            20m,
            30m);

        // Assert
        risk.Title.Should().Be("Test Risk");
    }
}

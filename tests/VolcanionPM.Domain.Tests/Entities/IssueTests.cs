using FluentAssertions;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Domain.Tests.Entities;

public class IssueTests
{
    private static Issue CreateValidIssue()
    {
        return Issue.Create(
            "Critical Bug in Payment Module",
            "Payment processing fails for international transactions",
            Guid.NewGuid(),
            IssueSeverity.Critical,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TestUser");
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateIssue()
    {
        // Arrange
        var title = "Login Button Not Working";
        var description = "Users cannot click the login button on mobile devices";
        var projectId = Guid.NewGuid();
        var severity = IssueSeverity.High;
        var reportedById = Guid.NewGuid();
        var assignedToId = Guid.NewGuid();

        // Act
        var issue = Issue.Create(title, description, projectId, severity, reportedById, assignedToId, "TestUser");

        // Assert
        issue.Should().NotBeNull();
        issue.Title.Should().Be(title);
        issue.Description.Should().Be(description);
        issue.ProjectId.Should().Be(projectId);
        issue.Severity.Should().Be(severity);
        issue.ReportedById.Should().Be(reportedById);
        issue.AssignedToId.Should().Be(assignedToId);
        issue.Status.Should().Be(IssueStatus.Open);
        issue.ResolvedDate.Should().BeNull();
        issue.Resolution.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Arrange & Act
        var action = () => Issue.Create(
            invalidTitle,
            "Valid description",
            Guid.NewGuid(),
            IssueSeverity.Medium,
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
        var action = () => Issue.Create(
            "Valid Title",
            invalidDescription,
            Guid.NewGuid(),
            IssueSeverity.Medium,
            null,
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*description*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateIssue()
    {
        // Arrange
        var issue = CreateValidIssue();
        var newTitle = "Updated Issue Title";
        var newDescription = "Updated description with more details";
        var newSeverity = IssueSeverity.Medium;

        // Act
        issue.Update(newTitle, newDescription, newSeverity, "UpdateUser");

        // Assert
        issue.Title.Should().Be(newTitle);
        issue.Description.Should().Be(newDescription);
        issue.Severity.Should().Be(newSeverity);
        issue.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void ChangeStatus_ToInProgress_ShouldUpdateStatus()
    {
        // Arrange
        var issue = CreateValidIssue();

        // Act
        issue.ChangeStatus(IssueStatus.InProgress, "StatusUser");

        // Assert
        issue.Status.Should().Be(IssueStatus.InProgress);
        issue.UpdatedBy.Should().Be("StatusUser");
    }

    [Fact]
    public void ChangeStatus_ToResolved_ShouldSetResolvedDate()
    {
        // Arrange
        var issue = CreateValidIssue();

        // Act
        issue.ChangeStatus(IssueStatus.Resolved, "ResolveUser");

        // Assert
        issue.Status.Should().Be(IssueStatus.Resolved);
        issue.ResolvedDate.Should().NotBeNull();
        issue.UpdatedBy.Should().Be("ResolveUser");
    }

    [Fact]
    public void ChangeStatus_ToClosed_ShouldSetResolvedDate()
    {
        // Arrange
        var issue = CreateValidIssue();

        // Act
        issue.ChangeStatus(IssueStatus.Closed, "CloseUser");

        // Assert
        issue.Status.Should().Be(IssueStatus.Closed);
        issue.ResolvedDate.Should().NotBeNull();
    }

    [Fact]
    public void ChangeStatus_ToSameStatus_ShouldDoNothing()
    {
        // Arrange
        var issue = CreateValidIssue();

        // Act
        issue.ChangeStatus(IssueStatus.Open, "User");

        // Assert
        issue.Status.Should().Be(IssueStatus.Open);
    }

    [Fact]
    public void Resolve_WithResolution_ShouldSetResolutionAndChangeStatus()
    {
        // Arrange
        var issue = CreateValidIssue();
        var resolution = "Fixed the payment gateway integration";

        // Act
        issue.Resolve(resolution, "ResolveUser");

        // Assert
        issue.Resolution.Should().Be(resolution);
        issue.Status.Should().Be(IssueStatus.Resolved);
        issue.ResolvedDate.Should().NotBeNull();
        issue.UpdatedBy.Should().Be("ResolveUser");
    }

    [Fact]
    public void Close_ShouldChangeStatusToClosed()
    {
        // Arrange
        var issue = CreateValidIssue();
        issue.Resolve("Fixed", "User");

        // Act
        issue.Close("CloseUser");

        // Assert
        issue.Status.Should().Be(IssueStatus.Closed);
        issue.UpdatedBy.Should().Be("CloseUser");
    }

    [Fact]
    public void Reopen_ShouldChangeStatusToReopenedAndClearResolvedDate()
    {
        // Arrange
        var issue = CreateValidIssue();
        issue.Resolve("Fixed", "User");

        // Act
        issue.Reopen("ReopenUser");

        // Assert
        issue.Status.Should().Be(IssueStatus.Reopened);
        issue.ResolvedDate.Should().BeNull();
        issue.UpdatedBy.Should().Be("ReopenUser");
    }

    [Fact]
    public void AssignTo_ShouldUpdateAssignedToId()
    {
        // Arrange
        var issue = CreateValidIssue();
        var newAssigneeId = Guid.NewGuid();

        // Act
        issue.AssignTo(newAssigneeId, "AssignUser");

        // Assert
        issue.AssignedToId.Should().Be(newAssigneeId);
        issue.UpdatedBy.Should().Be("AssignUser");
    }

    [Fact]
    public void Create_WithNullOptionalParameters_ShouldSucceed()
    {
        // Arrange & Act
        var issue = Issue.Create(
            "Test Issue",
            "Test description",
            Guid.NewGuid(),
            IssueSeverity.Low,
            null, // reportedById
            null, // assignedToId
            "TestUser");

        // Assert
        issue.ReportedById.Should().BeNull();
        issue.AssignedToId.Should().BeNull();
    }

    [Fact]
    public void Create_WithWhitespaceTitle_ShouldTrimTitle()
    {
        // Arrange
        var title = "  Test Issue  ";

        // Act
        var issue = Issue.Create(
            title,
            "Description",
            Guid.NewGuid(),
            IssueSeverity.Low);

        // Assert
        issue.Title.Should().Be("Test Issue");
    }

    [Fact]
    public void Create_WithDifferentSeverities_ShouldSetCorrectSeverity()
    {
        // Arrange & Act
        var lowIssue = Issue.Create("Low", "Description", Guid.NewGuid(), IssueSeverity.Low);
        var mediumIssue = Issue.Create("Medium", "Description", Guid.NewGuid(), IssueSeverity.Medium);
        var highIssue = Issue.Create("High", "Description", Guid.NewGuid(), IssueSeverity.High);
        var criticalIssue = Issue.Create("Critical", "Description", Guid.NewGuid(), IssueSeverity.Critical);

        // Assert
        lowIssue.Severity.Should().Be(IssueSeverity.Low);
        mediumIssue.Severity.Should().Be(IssueSeverity.Medium);
        highIssue.Severity.Should().Be(IssueSeverity.High);
        criticalIssue.Severity.Should().Be(IssueSeverity.Critical);
    }
}

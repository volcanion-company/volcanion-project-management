using FluentAssertions;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Domain.Tests.Entities;

public class TaskCommentTests
{
    private static TaskComment CreateValidComment()
    {
        return TaskComment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "This task looks good. Please proceed with implementation.",
            "TestUser");
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateTaskComment()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var content = "Great progress on this task! Keep up the good work.";

        // Act
        var comment = TaskComment.Create(taskId, authorId, content, "TestUser");

        // Assert
        comment.Should().NotBeNull();
        comment.TaskId.Should().Be(taskId);
        comment.AuthorId.Should().Be(authorId);
        comment.Content.Should().Be(content);
        comment.IsEdited.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidContent_ShouldThrowArgumentException(string invalidContent)
    {
        // Arrange & Act
        var action = () => TaskComment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            invalidContent,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*content*");
    }

    [Fact]
    public void Update_WithValidContent_ShouldUpdateComment()
    {
        // Arrange
        var comment = CreateValidComment();
        var newContent = "Updated comment with additional details.";

        // Act
        comment.Update(newContent, "UpdateUser");

        // Assert
        comment.Content.Should().Be(newContent);
        comment.IsEdited.Should().BeTrue();
        comment.UpdatedBy.Should().Be("UpdateUser");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_WithInvalidContent_ShouldThrowArgumentException(string invalidContent)
    {
        // Arrange
        var comment = CreateValidComment();

        // Act
        var action = () => comment.Update(invalidContent, "UpdateUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*content*");
    }

    [Fact]
    public void Update_MultipleTimesOnly_ShouldKeepIsEditedTrue()
    {
        // Arrange
        var comment = CreateValidComment();

        // Act
        comment.Update("First update", "User1");
        comment.Update("Second update", "User2");
        comment.Update("Third update", "User3");

        // Assert
        comment.IsEdited.Should().BeTrue();
        comment.Content.Should().Be("Third update");
        comment.UpdatedBy.Should().Be("User3");
    }

    [Fact]
    public void Create_WithWhitespaceContent_ShouldTrimContent()
    {
        // Arrange
        var content = "  This is a comment with extra whitespace  ";

        // Act
        var comment = TaskComment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            content,
            "TestUser");

        // Assert
        comment.Content.Should().Be("This is a comment with extra whitespace");
    }

    [Fact]
    public void Update_WithWhitespaceContent_ShouldTrimContent()
    {
        // Arrange
        var comment = CreateValidComment();
        var newContent = "  Updated with whitespace  ";

        // Act
        comment.Update(newContent, "UpdateUser");

        // Assert
        comment.Content.Should().Be("Updated with whitespace");
    }

    [Fact]
    public void Create_NewComment_ShouldNotBeEdited()
    {
        // Arrange & Act
        var comment = TaskComment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Brand new comment",
            "TestUser");

        // Assert
        comment.IsEdited.Should().BeFalse();
    }

    [Fact]
    public void Create_WithLongContent_ShouldSucceed()
    {
        // Arrange
        var longContent = string.Concat(Enumerable.Repeat("This is a very long comment. ", 50));

        // Act
        var comment = TaskComment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            longContent,
            "TestUser");

        // Assert
        comment.Content.Should().Be(longContent.Trim());
    }

    [Fact]
    public void Update_AfterCreate_ShouldSetIsEditedToTrue()
    {
        // Arrange
        var comment = TaskComment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Original content",
            "TestUser");
        
        comment.IsEdited.Should().BeFalse();

        // Act
        comment.Update("Modified content", "UpdateUser");

        // Assert
        comment.IsEdited.Should().BeTrue();
    }
}

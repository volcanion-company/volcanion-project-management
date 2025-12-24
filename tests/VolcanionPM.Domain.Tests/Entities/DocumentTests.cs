using FluentAssertions;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Domain.Tests.Entities;

public class DocumentTests
{
    private static Document CreateValidDocument()
    {
        return Document.Create(
            "Project Requirements.pdf",
            DocumentType.Requirements,
            "/documents/project-requirements.pdf",
            ".pdf",
            1024000,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Main requirements document",
            "1.0",
            "TestUser");
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateDocument()
    {
        // Arrange
        var name = "Design Mockups.png";
        var type = DocumentType.Design;
        var filePath = "/uploads/mockups.png";
        var fileExtension = ".png";
        var fileSize = 512000L;
        var projectId = Guid.NewGuid();
        var uploadedById = Guid.NewGuid();
        var description = "UI mockups for dashboard";
        var version = "2.1";

        // Act
        var document = Document.Create(name, type, filePath, fileExtension, fileSize, 
            projectId, uploadedById, description, version, "TestUser");

        // Assert
        document.Should().NotBeNull();
        document.Name.Should().Be(name);
        document.Type.Should().Be(type);
        document.FilePath.Should().Be(filePath);
        document.FileExtension.Should().Be("png"); // Without dot
        document.FileSize.Should().Be(fileSize);
        document.ProjectId.Should().Be(projectId);
        document.UploadedById.Should().Be(uploadedById);
        document.Description.Should().Be(description);
        document.Version.Should().Be(version);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange & Act
        var action = () => Document.Create(
            invalidName,
            DocumentType.Requirements,
            "/path/file.pdf",
            ".pdf",
            1000,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidFilePath_ShouldThrowArgumentException(string invalidPath)
    {
        // Arrange & Act
        var action = () => Document.Create(
            "Test Document",
            DocumentType.Requirements,
            invalidPath,
            ".pdf",
            1000,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*path*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Create_WithInvalidFileSize_ShouldThrowArgumentException(long invalidSize)
    {
        // Arrange & Act
        var action = () => Document.Create(
            "Test Document",
            DocumentType.Requirements,
            "/path/file.pdf",
            ".pdf",
            invalidSize,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "TestUser");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*File size must be positive*");
    }

    [Fact]
    public void Create_ShouldNormalizeFileExtension()
    {
        // Arrange & Act - with dot prefix
        var doc1 = Document.Create("File", DocumentType.Other, "/path", ".PDF", 1000, Guid.NewGuid(), Guid.NewGuid());
        
        // Without dot prefix
        var doc2 = Document.Create("File", DocumentType.Other, "/path", "jpg", 1000, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        doc1.FileExtension.Should().Be("pdf"); // lowercase, no dot
        doc2.FileExtension.Should().Be("jpg"); // lowercase
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateDocument()
    {
        // Arrange
        var document = CreateValidDocument();
        var newName = "Updated Requirements.pdf";
        var newDescription = "Updated description";
        var newType = DocumentType.TechnicalSpec;

        // Act
        document.Update(newName, newDescription, newType, "UpdateUser");

        // Assert
        document.Name.Should().Be(newName);
        document.Description.Should().Be(newDescription);
        document.Type.Should().Be(newType);
        document.UpdatedBy.Should().Be("UpdateUser");
    }

    [Fact]
    public void UpdateVersion_ShouldUpdateVersionNumber()
    {
        // Arrange
        var document = CreateValidDocument();

        // Act
        document.UpdateVersion("2.0", "UpdateUser");

        // Assert
        document.Version.Should().Be("2.0");
        document.UpdatedBy.Should().Be("UpdateUser");
    }

    [Theory]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(2097152, "2 MB")]
    [InlineData(1073741824, "1 GB")]
    [InlineData(100, "100 B")]
    public void GetFileSizeFormatted_ShouldFormatCorrectly(long fileSize, string expected)
    {
        // Arrange
        var document = Document.Create("File", DocumentType.Other, "/path", ".txt", 
            fileSize, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var formatted = document.GetFileSizeFormatted();

        // Assert
        formatted.Should().Be(expected);
    }

    [Fact]
    public void Create_WithNullOptionalParameters_ShouldSucceed()
    {
        // Arrange & Act
        var document = Document.Create(
            "Test.pdf",
            DocumentType.Other,
            "/path/test.pdf",
            ".pdf",
            1000,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null, // description
            null, // version
            "TestUser");

        // Assert
        document.Description.Should().BeNull();
        document.Version.Should().BeNull();
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldTrimName()
    {
        // Arrange
        var name = "  Test Document  ";

        // Act
        var document = Document.Create(
            name,
            DocumentType.Other,
            "/path",
            ".pdf",
            1000,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        document.Name.Should().Be("Test Document");
    }
}

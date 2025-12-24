using FluentAssertions;
using Moq;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Features.Users.Commands.Create;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Tests.Features.Users.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUser()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecurePassword123!",
            OrganizationId = Guid.NewGuid(),
            Role = UserRole.Developer,
            PhoneNumber = "+1234567890"
        };

        var hashedPassword = "hashed_password_123";

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        _passwordHasherMock.Verify(x => x.HashPassword(command.Password), Times.Once);
        _userRepositoryMock.Verify(
            x => x.AddAsync(It.Is<User>(u => 
                u.Email.Value == command.Email.ToLowerInvariant() && 
                u.FirstName == command.FirstName &&
                u.LastName == command.LastName), 
            It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "existing@example.com",
            Password = "Password123!",
            OrganizationId = Guid.NewGuid(),
            Role = UserRole.Developer
        };

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("email already exists");

        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "PlainTextPassword",
            OrganizationId = Guid.NewGuid(),
            Role = UserRole.ProjectManager
        };

        var hashedPassword = "super_secure_hashed_password";

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasherMock.Verify(x => x.HashPassword(command.Password), Times.Once);
        _userRepositoryMock.Verify(
            x => x.AddAsync(It.Is<User>(u => true), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(UserRole.Developer)]
    [InlineData(UserRole.ProjectManager)]
    [InlineData(UserRole.TeamLead)]
    [InlineData(UserRole.Designer)]
    public async Task Handle_WithDifferentRoles_ShouldCreateSuccessfully(UserRole role)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test.{role}@example.com",
            Password = "Password123!",
            OrganizationId = Guid.NewGuid(),
            Role = role
        };

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNullPhoneNumber_ShouldCreateSuccessfully()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            OrganizationId = Guid.NewGuid(),
            Role = UserRole.Developer,
            PhoneNumber = null
        };

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}

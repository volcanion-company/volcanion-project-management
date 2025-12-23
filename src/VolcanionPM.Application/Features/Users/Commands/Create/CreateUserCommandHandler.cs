using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Features.Users.Commands.Create;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result<Guid>.Failure("User with this email already exists");
        }

        var user = Domain.Entities.User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            _passwordHasher.HashPassword(request.Password),
            request.OrganizationId,
            request.Role,
            request.PhoneNumber,
            "System");

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}

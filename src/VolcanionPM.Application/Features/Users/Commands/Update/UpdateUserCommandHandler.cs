using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Users.Commands.Update;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<Unit>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            return Result<Unit>.Failure("User not found");
        }

        user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber, "System");
        user.ChangeRole(request.Role, "System");
        
        if (!request.IsActive && user.IsActive)
        {
            user.Deactivate("System");
        }
        else if (request.IsActive && !user.IsActive)
        {
            user.Activate("System");
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

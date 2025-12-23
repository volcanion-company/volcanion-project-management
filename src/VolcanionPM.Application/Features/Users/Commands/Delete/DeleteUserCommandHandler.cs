using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Users.Commands.Delete;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<Unit>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            return Result<Unit>.Failure("User not found");
        }

        // Soft delete by deactivating
        user.Deactivate("System");
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

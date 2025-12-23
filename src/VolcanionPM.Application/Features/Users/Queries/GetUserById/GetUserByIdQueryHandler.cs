using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Users.DTOs;

namespace VolcanionPM.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;

    public GetUserByIdQueryHandler(
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found");
        }

        // Get organization separately
        var organization = await _organizationRepository.GetByIdAsync(user.OrganizationId, cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email.Value,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            OrganizationId = user.OrganizationId,
            OrganizationName = organization?.Name,
            CreatedDate = user.CreatedAt,
            LastModifiedDate = user.UpdatedAt
        };

        return Result<UserDto>.Success(userDto);
    }
}

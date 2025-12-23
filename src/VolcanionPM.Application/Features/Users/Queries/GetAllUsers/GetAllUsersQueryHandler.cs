using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Users.DTOs;

namespace VolcanionPM.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<UserDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;

    public GetAllUsersQueryHandler(
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<List<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.User> users;

        if (request.OrganizationId.HasValue)
        {
            users = await _userRepository.GetByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken);
        }
        else
        {
            users = await _userRepository.GetAllAsync(cancellationToken);
        }

        // Filter by IsActive if specified
        if (request.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == request.IsActive.Value);
        }

        // Get unique organization IDs
        var organizationIds = users.Select(u => u.OrganizationId).Distinct();
        var organizations = new Dictionary<Guid, string>();

        // Fetch all organizations in batch
        foreach (var orgId in organizationIds)
        {
            var org = await _organizationRepository.GetByIdAsync(orgId, cancellationToken);
            if (org != null)
            {
                organizations[orgId] = org.Name;
            }
        }

        var userDtos = users.Select(user => new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email.Value,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            OrganizationId = user.OrganizationId,
            OrganizationName = organizations.GetValueOrDefault(user.OrganizationId),
            CreatedDate = user.CreatedAt,
            LastModifiedDate = user.UpdatedAt
        }).ToList();

        return Result<List<UserDto>>.Success(userDtos);
    }
}

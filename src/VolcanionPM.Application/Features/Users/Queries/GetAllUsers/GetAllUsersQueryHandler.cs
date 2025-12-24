using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Users.DTOs;

namespace VolcanionPM.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PagedResult<UserDto>>>
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

    public async Task<Result<PagedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userRepository.GetQueryable();

        // Filter by OrganizationId
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(u => u.OrganizationId == request.OrganizationId.Value);
        }

        // Filter by IsActive
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        // Filter by Role
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            query = query.Where(u => u.Role.ToString().ToLower() == request.Role.ToLower());
        }

        // Search in FirstName, LastName, Email
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower) ||
                u.Email.Value.ToLower().Contains(searchLower));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // Get total count
        var totalCount = query.Count();

        // Apply pagination
        var users = query.Skip(request.Skip).Take(request.Take).ToList();

        // Get unique organization IDs
        var organizationIds = users.Select(u => u.OrganizationId).Distinct().ToList();
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

        var pagedResult = PagedResult<UserDto>.Create(userDtos, request.Page, request.PageSize, totalCount);
        return Result<PagedResult<UserDto>>.Success(pagedResult);
    }

    private IQueryable<Domain.Entities.User> ApplySorting(
        IQueryable<Domain.Entities.User> query,
        string sortBy,
        string sortOrder)
    {
        var isDescending = sortOrder.ToLower() == "desc";

        query = sortBy.ToLower() switch
        {
            "firstname" => isDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
            "email" => isDescending ? query.OrderByDescending(u => u.Email.Value) : query.OrderBy(u => u.Email.Value),
            "role" => isDescending ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
            "isactive" => isDescending ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
            "createdat" => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        return query;
    }
}

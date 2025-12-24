using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Organizations.DTOs;

namespace VolcanionPM.Application.Features.Organizations.Queries.GetAllOrganizations;

public class GetAllOrganizationsQueryHandler : IRequestHandler<GetAllOrganizationsQuery, Result<PagedResult<OrganizationDto>>>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetAllOrganizationsQueryHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<PagedResult<OrganizationDto>>> Handle(GetAllOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var query = _organizationRepository.GetQueryable();

        // Filter by IsActive
        if (request.IsActive.HasValue)
        {
            query = query.Where(o => o.IsActive == request.IsActive.Value);
        }

        // Search in Name, Description
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(o =>
                o.Name.ToLower().Contains(searchLower) ||
                (o.Description != null && o.Description.ToLower().Contains(searchLower)));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // Get total count
        var totalCount = query.Count();

        // Apply pagination
        var organizations = query.Skip(request.Skip).Take(request.Take).ToList();

        var orgDtos = organizations.Select(organization => new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            Website = organization.Website,
            LogoUrl = organization.LogoUrl,
            Address = organization.Address != null ? new AddressDto
            {
                Street = organization.Address.Street,
                City = organization.Address.City,
                State = organization.Address.State,
                PostalCode = organization.Address.PostalCode,
                Country = organization.Address.Country
            } : null,
            IsActive = organization.IsActive,
            SubscriptionExpiryDate = organization.SubscriptionExpiryDate,
            UserCount = organization.Users.Count,
            ProjectCount = organization.Projects.Count,
            CreatedDate = organization.CreatedAt,
            LastModifiedDate = organization.UpdatedAt
        }).ToList();

        var pagedResult = PagedResult<OrganizationDto>.Create(orgDtos, request.Page, request.PageSize, totalCount);
        return Result<PagedResult<OrganizationDto>>.Success(pagedResult);
    }

    private IQueryable<Domain.Entities.Organization> ApplySorting(
        IQueryable<Domain.Entities.Organization> query,
        string sortBy,
        string sortOrder)
    {
        var isDescending = sortOrder.ToLower() == "desc";

        query = sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(o => o.Name) : query.OrderBy(o => o.Name),
            "isactive" => isDescending ? query.OrderByDescending(o => o.IsActive) : query.OrderBy(o => o.IsActive),
            "createdat" => isDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt),
            "subscriptionexpirydate" => isDescending ? query.OrderByDescending(o => o.SubscriptionExpiryDate) : query.OrderBy(o => o.SubscriptionExpiryDate),
            _ => query.OrderBy(o => o.Name)
        };

        return query;
    }
}

using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Organizations.DTOs;

namespace VolcanionPM.Application.Features.Organizations.Queries.GetAllOrganizations;

public class GetAllOrganizationsQueryHandler : IRequestHandler<GetAllOrganizationsQuery, Result<List<OrganizationDto>>>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetAllOrganizationsQueryHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<List<OrganizationDto>>> Handle(GetAllOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var organizations = await _organizationRepository.GetAllAsync(cancellationToken);

        if (request.IsActive.HasValue)
        {
            organizations = organizations.Where(o => o.IsActive == request.IsActive.Value);
        }

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

        return Result<List<OrganizationDto>>.Success(orgDtos);
    }
}

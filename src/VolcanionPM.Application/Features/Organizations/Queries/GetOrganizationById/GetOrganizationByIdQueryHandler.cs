using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Organizations.DTOs;

namespace VolcanionPM.Application.Features.Organizations.Queries.GetOrganizationById;

public class GetOrganizationByIdQueryHandler : IRequestHandler<GetOrganizationByIdQuery, Result<OrganizationDto>>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetOrganizationByIdQueryHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<OrganizationDto>> Handle(GetOrganizationByIdQuery request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (organization == null)
        {
            return Result<OrganizationDto>.Failure("Organization not found");
        }

        var orgDto = new OrganizationDto
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
        };

        return Result<OrganizationDto>.Success(orgDto);
    }
}

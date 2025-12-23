using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Features.Organizations.Commands.Create;

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Result<Guid>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        Address? address = null;
        if (request.Address != null)
        {
            address = Address.Create(
                request.Address.Street,
                request.Address.City,
                request.Address.State,
                request.Address.PostalCode,
                request.Address.Country);
        }

        var organization = Organization.Create(
            request.Name,
            request.Description,
            request.Website,
            address,
            "System");

        await _organizationRepository.AddAsync(organization, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(organization.Id);
    }
}

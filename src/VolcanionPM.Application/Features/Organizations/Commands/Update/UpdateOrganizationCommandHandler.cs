using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Features.Organizations.Commands.Update;

public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, Result<Unit>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (organization == null)
        {
            return Result<Unit>.Failure("Organization not found");
        }

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

        organization.UpdateDetails(
            request.Name,
            request.Description,
            request.Website,
            address,
            "System");

        if (!request.IsActive && organization.IsActive)
        {
            organization.Deactivate("System");
        }
        else if (request.IsActive && !organization.IsActive)
        {
            organization.Activate("System");
        }

        _organizationRepository.Update(organization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

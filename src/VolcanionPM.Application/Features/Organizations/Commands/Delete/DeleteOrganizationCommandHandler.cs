using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Organizations.Commands.Delete;

public class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, Result<Unit>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (organization == null)
        {
            return Result<Unit>.Failure("Organization not found");
        }

        organization.Deactivate("System");
        _organizationRepository.Update(organization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

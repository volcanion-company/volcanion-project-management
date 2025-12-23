using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.ResourceAllocations.Commands.Delete;

public class DeleteResourceAllocationCommandHandler : IRequestHandler<DeleteResourceAllocationCommand, Result<Unit>>
{
    private readonly IResourceAllocationRepository _resourceAllocationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteResourceAllocationCommandHandler(
        IResourceAllocationRepository resourceAllocationRepository,
        IUnitOfWork unitOfWork)
    {
        _resourceAllocationRepository = resourceAllocationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteResourceAllocationCommand request, CancellationToken cancellationToken)
    {
        var allocation = await _resourceAllocationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (allocation == null)
        {
            return Result<Unit>.Failure("Resource allocation not found");
        }

        allocation.MarkAsDeleted("System");
        _resourceAllocationRepository.Update(allocation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

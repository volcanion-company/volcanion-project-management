using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Features.ResourceAllocations.Commands.Update;

public class UpdateResourceAllocationCommandHandler : IRequestHandler<UpdateResourceAllocationCommand, Result<Unit>>
{
    private readonly IResourceAllocationRepository _resourceAllocationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateResourceAllocationCommandHandler(
        IResourceAllocationRepository resourceAllocationRepository,
        IUnitOfWork unitOfWork)
    {
        _resourceAllocationRepository = resourceAllocationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateResourceAllocationCommand request, CancellationToken cancellationToken)
    {
        var allocation = await _resourceAllocationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (allocation == null)
        {
            return Result<Unit>.Failure("Resource allocation not found");
        }

        try
        {
            var dateRange = DateRange.Create(request.StartDate, request.EndDate);
            Money? hourlyRate = null;

            if (request.HourlyRateAmount.HasValue && !string.IsNullOrWhiteSpace(request.HourlyRateCurrency))
            {
                hourlyRate = Money.Create(request.HourlyRateAmount.Value, request.HourlyRateCurrency);
            }

            allocation.Update(
                dateRange,
                request.Type,
                request.AllocationPercentage,
                hourlyRate,
                request.Notes,
                "System");

            _resourceAllocationRepository.Update(allocation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (ArgumentException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }
    }
}

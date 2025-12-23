using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Features.ResourceAllocations.Commands.Create;

public class CreateResourceAllocationCommandHandler : IRequestHandler<CreateResourceAllocationCommand, Result<Guid>>
{
    private readonly IResourceAllocationRepository _resourceAllocationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateResourceAllocationCommandHandler(
        IResourceAllocationRepository resourceAllocationRepository,
        IUserRepository userRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _resourceAllocationRepository = resourceAllocationRepository;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateResourceAllocationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<Guid>.Failure("User not found");
        }

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<Guid>.Failure("Project not found");
        }

        try
        {
            var dateRange = DateRange.Create(request.StartDate, request.EndDate);
            Money? hourlyRate = null;

            if (request.HourlyRateAmount.HasValue && !string.IsNullOrWhiteSpace(request.HourlyRateCurrency))
            {
                hourlyRate = Money.Create(request.HourlyRateAmount.Value, request.HourlyRateCurrency);
            }

            var allocation = ResourceAllocation.Create(
                request.UserId,
                request.ProjectId,
                dateRange,
                request.Type,
                request.AllocationPercentage,
                hourlyRate,
                request.Notes,
                "System");

            await _resourceAllocationRepository.AddAsync(allocation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(allocation.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

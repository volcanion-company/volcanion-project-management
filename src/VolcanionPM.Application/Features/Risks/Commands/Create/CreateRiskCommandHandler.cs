using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Features.Risks.Commands.Create;

public class CreateRiskCommandHandler : IRequestHandler<CreateRiskCommand, Result<Guid>>
{
    private readonly IRiskRepository _riskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRiskCommandHandler(
        IRiskRepository riskRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _riskRepository = riskRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateRiskCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<Guid>.Failure("Project not found");
        }

        try
        {
            var risk = Risk.Create(
                request.Title,
                request.Description,
                request.Level,
                request.ProjectId,
                request.Probability,
                request.Impact,
                request.OwnerId,
                request.MitigationStrategy,
                "System");

            await _riskRepository.AddAsync(risk, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(risk.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

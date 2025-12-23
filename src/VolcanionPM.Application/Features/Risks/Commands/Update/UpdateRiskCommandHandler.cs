using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Risks.Commands.Update;

public class UpdateRiskCommandHandler : IRequestHandler<UpdateRiskCommand, Result<Unit>>
{
    private readonly IRiskRepository _riskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRiskCommandHandler(
        IRiskRepository riskRepository,
        IUnitOfWork unitOfWork)
    {
        _riskRepository = riskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateRiskCommand request, CancellationToken cancellationToken)
    {
        var risk = await _riskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (risk == null)
        {
            return Result<Unit>.Failure("Risk not found");
        }

        try
        {
            risk.Update(
                request.Title,
                request.Description,
                request.Level,
                request.Probability,
                request.Impact,
                request.MitigationStrategy,
                "System");

            _riskRepository.Update(risk);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (ArgumentException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }
    }
}

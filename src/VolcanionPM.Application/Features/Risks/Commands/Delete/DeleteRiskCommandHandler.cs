using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Risks.Commands.Delete;

public class DeleteRiskCommandHandler : IRequestHandler<DeleteRiskCommand, Result<Unit>>
{
    private readonly IRiskRepository _riskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRiskCommandHandler(
        IRiskRepository riskRepository,
        IUnitOfWork unitOfWork)
    {
        _riskRepository = riskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteRiskCommand request, CancellationToken cancellationToken)
    {
        var risk = await _riskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (risk == null)
        {
            return Result<Unit>.Failure("Risk not found");
        }

        risk.MarkAsDeleted("System");
        _riskRepository.Update(risk);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

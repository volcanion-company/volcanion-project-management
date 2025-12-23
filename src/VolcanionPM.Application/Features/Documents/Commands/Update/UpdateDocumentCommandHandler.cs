using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Documents.Commands.Update;

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, Result<Unit>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (document == null)
        {
            return Result<Unit>.Failure("Document not found");
        }

        try
        {
            document.Update(
                request.Name,
                request.Description,
                request.Type,
                "System");

            _documentRepository.Update(document);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (ArgumentException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }
    }
}

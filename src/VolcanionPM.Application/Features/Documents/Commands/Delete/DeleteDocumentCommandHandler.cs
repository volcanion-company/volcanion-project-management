using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Documents.Commands.Delete;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result<Unit>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (document == null)
        {
            return Result<Unit>.Failure("Document not found");
        }

        document.MarkAsDeleted("System");
        _documentRepository.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}

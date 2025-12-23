using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Features.Documents.Commands.Create;

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Result<Guid>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<Guid>.Failure("Project not found");
        }

        try
        {
            var document = Document.Create(
                request.Name,
                request.Type,
                request.FilePath,
                request.FileExtension,
                request.FileSize,
                request.ProjectId,
                request.UploadedById,
                request.Description,
                request.Version,
                "System");

            await _documentRepository.AddAsync(document, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(document.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

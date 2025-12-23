using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Documents.DTOs;

namespace VolcanionPM.Application.Features.Documents.Queries.GetDocumentById;

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, Result<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentByIdQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Result<DocumentDto>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (document == null)
        {
            return Result<DocumentDto>.Failure("Document not found");
        }

        var dto = new DocumentDto
        {
            Id = document.Id,
            Name = document.Name,
            Description = document.Description,
            Type = document.Type,
            FilePath = document.FilePath,
            FileExtension = document.FileExtension,
            FileSize = document.FileSize,
            FileSizeFormatted = document.GetFileSizeFormatted(),
            Version = document.Version,
            ProjectId = document.ProjectId,
            ProjectName = document.Project?.Name,
            UploadedById = document.UploadedById,
            UploadedByName = document.UploadedBy?.GetFullName(),
            CreatedDate = document.CreatedAt,
            LastModifiedDate = document.UpdatedAt
        };

        return Result<DocumentDto>.Success(dto);
    }
}

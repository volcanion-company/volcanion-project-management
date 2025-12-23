using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Documents.DTOs;

namespace VolcanionPM.Application.Features.Documents.Queries.GetDocumentsByProject;

public class GetDocumentsByProjectQueryHandler : IRequestHandler<GetDocumentsByProjectQuery, Result<List<DocumentDto>>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentsByProjectQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Result<List<DocumentDto>>> Handle(GetDocumentsByProjectQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        var dtos = documents.Select(document => new DocumentDto
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
        }).ToList();

        return Result<List<DocumentDto>>.Success(dtos);
    }
}

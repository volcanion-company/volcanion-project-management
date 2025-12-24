using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Documents.DTOs;

namespace VolcanionPM.Application.Features.Documents.Queries.GetDocumentsByProject;

public class GetDocumentsByProjectQueryHandler : IRequestHandler<GetDocumentsByProjectQuery, Result<PagedResult<DocumentDto>>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentsByProjectQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Result<PagedResult<DocumentDto>>> Handle(GetDocumentsByProjectQuery request, CancellationToken cancellationToken)
    {
        var query = _documentRepository.GetQueryable()
            .Where(d => d.ProjectId == request.ProjectId);

        // Filter by Type
        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.Where(d => d.Type.ToString().ToLower() == request.Type.ToLower());
        }

        // Search in Name, Description
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.Name.ToLower().Contains(searchLower) ||
                (d.Description != null && d.Description.ToLower().Contains(searchLower)));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // Get total count
        var totalCount = query.Count();

        // Apply pagination
        var documents = query.Skip(request.Skip).Take(request.Take).ToList();

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

        var pagedResult = PagedResult<DocumentDto>.Create(dtos, request.Page, request.PageSize, totalCount);
        return Result<PagedResult<DocumentDto>>.Success(pagedResult);
    }

    private IQueryable<Domain.Entities.Document> ApplySorting(
        IQueryable<Domain.Entities.Document> query,
        string sortBy,
        string sortOrder)
    {
        var isDescending = sortOrder.ToLower() == "desc";

        query = sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            "type" => isDescending ? query.OrderByDescending(d => d.Type) : query.OrderBy(d => d.Type),
            "filesize" => isDescending ? query.OrderByDescending(d => d.FileSize) : query.OrderBy(d => d.FileSize),
            "createdat" => isDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        return query;
    }
}

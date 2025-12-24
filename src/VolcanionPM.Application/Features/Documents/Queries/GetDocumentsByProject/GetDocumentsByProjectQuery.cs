using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Documents.DTOs;

namespace VolcanionPM.Application.Features.Documents.Queries.GetDocumentsByProject;

public class GetDocumentsByProjectQuery : PagedQuery, IRequest<Result<PagedResult<DocumentDto>>>
{
    public Guid ProjectId { get; set; }
    public string? Type { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "createdat";
    public string SortOrder { get; set; } = "desc";
}

using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Common.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> GetByUploadedByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> GetByTypeAsync(DocumentType type, CancellationToken cancellationToken = default);
}

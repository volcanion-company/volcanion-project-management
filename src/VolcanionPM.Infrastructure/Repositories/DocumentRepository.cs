using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Document>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .Where(d => d.ProjectId == projectId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByUploadedByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .Where(d => d.UploadedById == userId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByTypeAsync(DocumentType type, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .Where(d => d.Type == type && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

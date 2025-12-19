using Microsoft.EntityFrameworkCore;
using ToDo.Application.DTOs.Status;
using ToDo.Infrastructure.Contexts;
using ToDo.Domain.Entities;

namespace ToDo.API.Services;

public sealed class ProjectStatusHistoryService: IProjectStatusHistoryService
{

    private readonly AppDbContext _db;

    public ProjectStatusHistoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task TransitionAsync(Guid projectId, Guid toProjectStatusId, string actorUserId, string? note)
    {
        actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;

        
        var toStatus = await _db.ProjectStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == toProjectStatusId && x.ProjectId == projectId);

        if (toStatus is null)
            throw new KeyNotFoundException("ToProjectStatusId not found for this project.");

        
        var last = await _db.ProjectStatusHistories
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => new { x.ToProjectStatusId })
            .FirstOrDefaultAsync();

        Guid? fromProjectStatusId = last?.ToProjectStatusId;

        
        if (fromProjectStatusId.HasValue && fromProjectStatusId.Value == toProjectStatusId)
            throw new InvalidOperationException("Project is already in this status.");

        var history = new ProjectStatusHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FromProjectStatusId = fromProjectStatusId,
            ToProjectStatusId = toProjectStatusId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            ChangedAt = DateTime.UtcNow,

            
            ChangedByUserId = actorUserId,

            IsDeleted = false
        };

        _db.ProjectStatusHistories.Add(history);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ProjectStatusHistoryResponse>> GetHistoryAsync(Guid projectId)
    {
        return await _db.ProjectStatusHistories
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => new ProjectStatusHistoryResponse
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                FromProjectStatusId = x.FromProjectStatusId,
                ToProjectStatusId = x.ToProjectStatusId,
                Note = x.Note,
                ChangedByUserId = x.ChangedByUserId,
                ChangedAt = x.ChangedAt
            })
            .ToListAsync();
    }
}

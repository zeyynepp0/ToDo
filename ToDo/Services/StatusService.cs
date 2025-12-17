using Microsoft.EntityFrameworkCore;
using ToDo.Application.DTOs;
using ToDo.Domain.Entities;
using ToDo.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using ToDo.Application.DTOs;
using ToDo.Domain.Entities;
using ToDo.API.Services;

namespace ToDo.API.Service
{
    public sealed class StatusService : IStatusService
    {
    private readonly AppDbContext _db;
    public StatusService(AppDbContext db)
    {
        _db = db;
    }

        //------------------- ToggleAsync Implementation ------------------//
        public async Task ToggleAsync(Guid projectId, Guid projectStatusId, bool isEnabled, string actorUserId)
        {
            var status = await _db.ProjectStatuses
             .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Id == projectStatusId);

            if (status is null)
                throw new KeyNotFoundException("Status not found for this project.");

            status.IsEnabled = isEnabled;
            await _db.SaveChangesAsync();
        }
        //------------------- AddCustomStatusAsync Implementation ------------------//
        public async Task<Guid> AddCustomStatusAsync(Guid projectId, string name, string actorUserId)
        {
            actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;

            var def = new StatusDefinition
            {
                Id = Guid.NewGuid(),
                Name = name,
                SystemCode = null,
                IsSystem = false,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedByuserId = actorUserId
            };

            _db.StatusDefinitions.Add(def);

           
            var maxOrder = await _db.ProjectStatuses
                .Where(x => x.ProjectId == projectId)
                .MaxAsync(x => (int?)x.OrderNo) ?? 0;

            var ps = new ProjectStatus
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                StatusDefinitionId = def.Id,
                OrderNo = maxOrder + 1,
                IsEnabled = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedByuserId = actorUserId
            };

            _db.ProjectStatuses.Add(ps);

            await _db.SaveChangesAsync();
            return ps.Id;
        }
        

        //------------------- GetProjectStatusesAsync Implementation ------------------//
        public async Task<List<ProjectStatusResponse>> GetProjectStatusesAsync(Guid projectId)
        {
            return await _db.ProjectStatuses
           .AsNoTracking()
           .Where(x => x.ProjectId == projectId)
           .OrderBy(x => x.OrderNo)
           .Select(x => new ProjectStatusResponse
           {
               ProjectStatusId = x.Id,
               StatusDefinitionId = x.StatusDefinitionId,
               Name = x.StatusDefinition.Name,
               SystemCode = x.StatusDefinition.SystemCode,
               IsSystem = x.StatusDefinition.IsSystem,
               IsEnabled = x.IsEnabled,
               OrderNo = x.OrderNo
           })
           .ToListAsync();
        }

        //------------------- ReorderAsync Implementation ------------------//
        public async Task ReorderAsync(Guid projectId, List<ReorderProjectStatusItem> items, string actorUserId)
        {
            var ids = items.Select(x => x.ProjectStatusId).ToList();

            var statuses = await _db.ProjectStatuses
                .Where(x => x.ProjectId == projectId && ids.Contains(x.Id))
                .ToListAsync();

            if (statuses.Count != items.Count)
                throw new KeyNotFoundException("One or more statuses were not found for this project.");

            using var tx = await _db.Database.BeginTransactionAsync();

            // Unique index varsa çakışmayı engellemek için temp order
            var temp = 100000;
            foreach (var s in statuses)
                s.OrderNo = temp++;

            await _db.SaveChangesAsync();

            foreach (var s in statuses)
                s.OrderNo = items.Single(i => i.ProjectStatusId == s.Id).OrderNo;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }

    }
}

using Microsoft.EntityFrameworkCore;
using ToDo.Domain.Entities;
using ToDo.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using ToDo.Domain.Entities;
using ToDo.API.Services;
using ToDo.Application.DTOs.Status;

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
            using var tx = await _db.Database.BeginTransactionAsync();
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
        //public async Task ReorderAsync(Guid projectId, List<ReorderProjectStatusItem> items, string actorUserId)
        //{
        //    var ids = items.Select(x => x.ProjectStatusId).ToList();

        //    var statuses = await _db.ProjectStatuses
        //        .Where(x => x.ProjectId == projectId && ids.Contains(x.Id))
        //        .ToListAsync();

        //    if (statuses.Count != items.Count)
        //        throw new KeyNotFoundException("One or more statuses were not found for this project.");

        //    using var tx = await _db.Database.BeginTransactionAsync();

        //    // Unique index varsa çakışmayı engellemek için temp order
        //    var temp = 100000;
        //    foreach (var s in statuses)
        //        s.OrderNo = temp++;

        //    await _db.SaveChangesAsync();

        //    foreach (var s in statuses)
        //        s.OrderNo = items.Single(i => i.ProjectStatusId == s.Id).OrderNo;

        //    await _db.SaveChangesAsync();
        //    await tx.CommitAsync();
        //}
        public async Task ReorderAsync(Guid projectId, List<ReorderProjectStatusItem> items, string actorUserId)
        {
            if (items == null || items.Count == 0)
                throw new InvalidOperationException("Reorder list cannot be empty.");

            var ids = items.Select(x => x.ProjectStatusId).ToList();

            var statuses = await _db.ProjectStatuses
                .Where(x => x.ProjectId == projectId && ids.Contains(x.Id))
                .ToListAsync();

            if (statuses.Count != items.Count)
                throw new KeyNotFoundException("One or more statuses were not found for this project.");

            //  OrderNo'lar 1,2,3... şeklinde olmalı
            var newOrders = items.Select(x => x.OrderNo).OrderBy(x => x).ToList();

            for (int i = 0; i < newOrders.Count; i++)
            {
                if (newOrders[i] != i + 1)
                    throw new InvalidOperationException(
                        "Status sıralaması 1'den başlayarak kesintisiz olmalıdır.");
            }

            //  Sadece bir önceki / bir sonraki yere taşınabilir
            foreach (var status in statuses)
            {
                var newOrder = items.Single(x => x.ProjectStatusId == status.Id).OrderNo;
                var oldOrder = status.OrderNo;

                if (Math.Abs(oldOrder - newOrder) > 1)
                {
                    throw new InvalidOperationException(
                        "Bir durum sadece bir önceki veya bir sonraki konuma taşınabilir.");
                }
            }

            using var tx = await _db.Database.BeginTransactionAsync();

            // Çakışmayı önlemek için geçici order
            var temp = 100000;
            foreach (var s in statuses)
                s.OrderNo = temp++;

            await _db.SaveChangesAsync();

            // Gerçek order’ları ver
            foreach (var s in statuses)
                s.OrderNo = items.Single(i => i.ProjectStatusId == s.Id).OrderNo;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task<Guid> AddTransitionAsync(CreateStatusTransitionRequest request, string actorUserId)
        {
            actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;
            // 1. Validasyon: Hedef statü var mı?
            var toDef = await _db.StatusDefinitions.FindAsync(request.ToStatusDefinitionId);
            if (toDef == null)
                throw new KeyNotFoundException("Hedef statü tanımı (ToStatusDefinition) bulunamadı.");

            // 2. Validasyon: Kaynak statü var mı? (Null değilse kontrol et)
            if (request.FromStatusDefinitionId.HasValue)
            {
                var fromDef = await _db.StatusDefinitions.FindAsync(request.FromStatusDefinitionId.Value);
                if (fromDef == null)
                    throw new KeyNotFoundException("Kaynak statü tanımı (FromStatusDefinition) bulunamadı.");

                // Kendine geçişi engellemek isterseniz:
                if (request.FromStatusDefinitionId == request.ToStatusDefinitionId)
                    throw new InvalidOperationException("Bir statüden kendisine geçiş kuralı tanımlanamaz.");
            }

            // 3. Çakışma Kontrolü: Bu kural zaten var mı?
            bool exists = await _db.StatusTransitions.AnyAsync(x =>
                x.FromStatusDefinitionId == request.FromStatusDefinitionId &&
                x.ToStatusDefinitionId == request.ToStatusDefinitionId &&
                !x.IsDeleted);

            if (exists)
                throw new InvalidOperationException("Bu geçiş kuralı zaten tanımlı.");

            // 4. Kayıt
            var transition = new StatusTransition
            {
                Id = Guid.NewGuid(),
                FromStatusDefinitionId = request.FromStatusDefinitionId,
                ToStatusDefinitionId = request.ToStatusDefinitionId,
                IsActive = true,
                CreatedByuserId = actorUserId,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
                // CreatedByUserId ekleyebilirsiniz context'ten geliyorsa
            };

            _db.StatusTransitions.Add(transition);
            await _db.SaveChangesAsync();

            return transition.Id;
        }

        public async Task RemoveTransitionAsync(Guid transitionId, string actorUserId)
        {
            actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;
            var transition = await _db.StatusTransitions.FindAsync(transitionId);
            if (transition == null)
                throw new KeyNotFoundException("Geçiş kuralı bulunamadı.");

            // Hard delete yerine Soft delete tercih edelim
            transition.IsDeleted = true;
            // Veya tamamen silmek isterseniz: _db.StatusTransitions.Remove(transition);

            await _db.SaveChangesAsync();
        }

        public async Task<List<StatusTransitionResponse>> GetTransitionsAsync( string actorUserId)
        {
            actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;

            return await _db.StatusTransitions
                 .AsNoTracking()
                 .Where(x => !x.IsDeleted) // Silinmişleri getirme
                 .Include(x => x.FromStatusDefinition)
                 .Include(x => x.ToStatusDefinition)
                 .OrderBy(x => x.FromStatusDefinition.Name) // Sıralama isteğe bağlı
                 .Select(x => new StatusTransitionResponse
                 {
                     Id = x.Id,
                     FromStatusDefinitionId = x.FromStatusDefinitionId,
                     FromStatusName = x.FromStatusDefinition != null ? x.FromStatusDefinition.Name : "Başlangıç (İlk Kayıt)",
                     ToStatusDefinitionId = x.ToStatusDefinitionId,
                     ToStatusName = x.ToStatusDefinition.Name,
                     IsActive = x.IsActive
                 })
                 .ToListAsync();
        }

        // ------------------- ChangeStatusAsync --------------------------------------------- //
        public async Task ChangeStatusAsync(Guid projectId, Guid toStatusDefinitionId, string actorUserId)
        {
            actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;

            var project = await _db.Projects //project+currentProjectStatus
                .Include(x=>x.CurrentProjectStatus)
                .FirstOrDefaultAsync(x => x.Id == projectId);

            if (project == null)
                throw new KeyNotFoundException("Proje bulunamadı.");

            // mevcut durumdaki StatusDefinition
            var fromStatusDefinitionId = project.CurrentProjectStatus?.StatusDefinitionId;

            //Transition kuralını kontrol et
            var allowed = await _db.StatusTransitions.AnyAsync(x =>
            x.IsActive && !x.IsDeleted && 
            x.FromStatusDefinitionId == fromStatusDefinitionId && 
            x.ToStatusDefinitionId == toStatusDefinitionId);

            if (!allowed)
                throw new InvalidOperationException( "Bu statü geçişine izin verilmiyor.");

            // proje bu statüye sahip mi kontrol ettt
            var toProjectStatus = await _db.ProjectStatuses
            .FirstOrDefaultAsync(x => x.ProjectId == projectId &&
            x.StatusDefinitionId == toStatusDefinitionId &&
            x.IsEnabled &&
            x.IsDeleted );

            if (toProjectStatus is null)
                throw new InvalidOperationException(
                    "Proje bu statüye sahip değil veya statü pasif.");

            using var tx = await _db.Database.BeginTransactionAsync();

            
            var history = new ProjectStatusHistory
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                FromProjectStatusId = project.CurrentProjectStatusId,
                ToProjectStatusId = toProjectStatus.Id,
                ChangedBy = actorUserId,
                ChangedAt = DateTime.UtcNow
            };

            _db.ProjectStatusHistories.Add(history);

            
            project.CurrentProjectStatusId = toProjectStatus.Id;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}

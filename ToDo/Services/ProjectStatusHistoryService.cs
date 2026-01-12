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

    //public async Task TransitionAsync(Guid projectId, Guid toProjectStatusId, string actorUserId, string? note)
    //{
    //    actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;

        
    //    var toStatus = await _db.ProjectStatuses
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(x => x.Id == toProjectStatusId && x.ProjectId == projectId);

    //    if (toStatus is null)
    //        throw new KeyNotFoundException("ToProjectStatusId not found for this project.");

        
    //    var last = await _db.ProjectStatusHistories
    //        .Where(x => x.ProjectId == projectId)
    //        .OrderByDescending(x => x.ChangedAt)
    //        .Select(x => new { x.ToProjectStatusId })
    //        .FirstOrDefaultAsync();

    //    Guid? fromProjectStatusId = last?.ToProjectStatusId;

        
    //    if (fromProjectStatusId.HasValue && fromProjectStatusId.Value == toProjectStatusId)
    //        throw new InvalidOperationException("Project is already in this status.");

    //    var history = new ProjectStatusHistory
    //    {
    //        Id = Guid.NewGuid(),
    //        ProjectId = projectId,
    //        FromProjectStatusId = fromProjectStatusId,
    //        ToProjectStatusId = toProjectStatusId,
    //        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
    //        ChangedAt = DateTime.UtcNow,

            
    //        ChangedByUserId = actorUserId,

    //        IsDeleted = false
    //    };

    //    _db.ProjectStatusHistories.Add(history);
    //    await _db.SaveChangesAsync();
    //}

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

    public async Task TransitionAsync(Guid projectId, Guid toProjectStatusId, string actorUserId, string? note)
    {
        actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;

        // 1. HEDEF DURUMU ÇEK (ToStatus)
        // Geçilmek istenen ProjectStatus'un hangi 'StatusDefinitionId'ye sahip olduğunu bulmamız gerekiyor.
        var toStatusInfo = await _db.ProjectStatuses
            .AsNoTracking()
            .Where(x => x.Id == toProjectStatusId && x.ProjectId == projectId)
            .Select(x => new { x.Id, x.StatusDefinitionId })
            .FirstOrDefaultAsync();

        if (toStatusInfo is null)
            throw new KeyNotFoundException("Hedef statü (ToProjectStatusId) bu proje için bulunamadı.");

        // 2. MEVCUT DURUMU BUL (Current Status)
        // Projenin en son hangi statüde olduğunu history tablosundan öğreniyoruz.
        var lastHistory = await _db.ProjectStatusHistories
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => new { x.ToProjectStatusId })
            .FirstOrDefaultAsync();

        Guid? fromProjectStatusId = lastHistory?.ToProjectStatusId;
        Guid? fromStatusDefId = null;

        // Eğer projenin bir geçmişi varsa (yani 'Yeni' değilse, bir yerden geliyorsa)
        if (fromProjectStatusId.HasValue)
        {
            // a) Aynı statüye tekrar geçmeye çalışıyorsa engelle
            if (fromProjectStatusId.Value == toProjectStatusId)
                throw new InvalidOperationException("Proje zaten bu statüde.");

            // b) Mevcut statünün 'StatusDefinitionId'sini bul
            var fromStatusInfo = await _db.ProjectStatuses
                .AsNoTracking()
                .Where(x => x.Id == fromProjectStatusId.Value)
                .Select(x => new { x.StatusDefinitionId })
                .FirstOrDefaultAsync();

            fromStatusDefId = fromStatusInfo?.StatusDefinitionId;
        }

        // 3. STATE MACHINE KONTROLÜ (Kritik Adım)
        // Veritabanındaki 'StatusTransitions' tablosuna bak: 
        // "Şu anki Tanım -> Hedef Tanım" şeklinde aktif bir geçiş kuralı var mı?
        // (Not: fromStatusDefId NULL ise, bu projenin ilk statüsü demektir.)

        var isValidTransition = await _db.StatusTransitions
            .AnyAsync(t =>
                t.IsActive &&
                t.FromStatusDefinitionId == fromStatusDefId &&
                t.ToStatusDefinitionId == toStatusInfo.StatusDefinitionId
            );

        if (!isValidTransition)
        {
            // Geçiş kuralı bulunamadıysa işlemi durdur.
            throw new InvalidOperationException("Mevcut durumdan hedeflenen duruma geçiş izni (kuralı) bulunmamaktadır.");
        }

        // 4. HISTORY KAYDI OLUŞTUR
        // Kontrolden geçtiyse kaydı ekle.
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

   
}

using ToDo.Application.DTOs.Status;

namespace ToDo.API.Services;

public interface IStatusService
{
    Task<List<ProjectStatusResponse>> GetProjectStatusesAsync(Guid projectId);
    Task ReorderAsync(Guid projectId, List<ReorderProjectStatusItem> items, string actorUserId);
    Task ToggleAsync(Guid projectId, Guid projectStatusId, bool isEnabled, string actorUserId);
    Task<Guid> AddCustomStatusAsync(Guid projectId, string name, string actorUserId);

    // Geçiş kuralı ekler (Örn: Analiz -> Geliştirme)
    Task<Guid> AddTransitionAsync(CreateStatusTransitionRequest request, string actorUserId);

    // Geçiş kuralını siler
    Task RemoveTransitionAsync(Guid transitionId, string actorUserId);

    // Tüm kuralları listeler (Admin paneli için)
    Task<List<StatusTransitionResponse>> GetTransitionsAsync(string actorUserId);
    Task ChangeStatusAsync(Guid projectId, Guid toStatusDefinitionId,string actorUserId);

}

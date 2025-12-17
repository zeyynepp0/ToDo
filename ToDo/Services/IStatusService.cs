using ToDo.Application.DTOs;

namespace ToDo.API.Services;

public interface IStatusService
{
    Task<List<ProjectStatusResponse>> GetProjectStatusesAsync(Guid projectId);
    Task ReorderAsync(Guid projectId, List<ReorderProjectStatusItem> items, string actorUserId);
    Task ToggleAsync(Guid projectId, Guid projectStatusId, bool isEnabled, string actorUserId);
    Task<Guid> AddCustomStatusAsync(Guid projectId, string name, string actorUserId);
}

using ToDo.Application.DTOs.Status;

namespace ToDo.API.Services;


public interface IProjectStatusHistoryService
{
    Task TransitionAsync(Guid projectId, Guid toProjectStatusId, string actorUserId, string? note);
    Task<List<ProjectStatusHistoryResponse>> GetHistoryAsync(Guid projectId);
}

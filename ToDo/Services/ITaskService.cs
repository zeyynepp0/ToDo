using ToDo.Application.DTOs.Tasks;

namespace ToDo.API.Services;

public interface ITaskService
{
    Task<Guid> CreateAsync(Guid projectId, CreateTaskRequest request, string actorUserId);
    Task<List<TaskTreeResponse>> GetTreeByStatusAsync(Guid projectId, Guid projectStatusId);
    Task MoveAsync(Guid projectId, Guid taskId, MoveTaskRequest request, string actorUserId);
    Task ReorderAsync(Guid projectId, Guid projectStatusId, Guid? parentTaskId, List <ReorderTaskItem> items, string actorUserId);
}
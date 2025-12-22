using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDo.API.Services;
using ToDo.Application.DTOs.Tasks;

namespace ToDo.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;

    public TasksController(ITaskService tasks)
    {
        _tasks = tasks;
    }

    //private string ActorUserId => "System";
    private string ActorUserId =>
    User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

    [HttpPost("tasks")]// Görev oluştur
    public async Task<ActionResult<Guid>> CreateTask([FromRoute] Guid projectId, [FromQuery] CreateTaskRequest request)
    {
        try
        {
            var id = await _tasks.CreateAsync(projectId, request, ActorUserId);
            return Ok(id);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    //  Status altındaki görevleri tree olarak getir
    [HttpGet("statuses/{projectStatusId:guid}/tasks-tree")]
    public async Task<ActionResult<List<TaskTreeResponse>>> GetTasksTree(Guid projectId, Guid projectStatusId)
    {
        try
        {
            var tree = await _tasks.GetTreeByStatusAsync(projectId, projectStatusId);
            return Ok(tree);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpPut("tasks/{taskId:guid}/move")]
    public async Task<IActionResult> MoveTask(Guid projectId, Guid taskId, [FromBody] MoveTaskRequest request)
    {
        try
        {
            await _tasks.MoveAsync(projectId, taskId, request, ActorUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("statuses/{projectStatusId:guid}/tasks/reorder")]
    public async Task<IActionResult> ReorderTasks(
        Guid projectId,
        Guid projectStatusId,
        [FromQuery] Guid parentTaskId,
        [FromBody] List<ReorderTaskItem> items)
    {
        try
        {
            await _tasks.ReorderAsync(projectId, projectStatusId, parentTaskId, items, ActorUserId);
            return NoContent();
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDo.API.Services;
using ToDo.Application.DTOs.Status;

namespace ToDo.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/status")]
public sealed class ProjectStatusHistoryController : Controller
{
    private readonly IProjectStatusHistoryService _history;

    public ProjectStatusHistoryController(IProjectStatusHistoryService history)
    {
        _history = history;
    }

    //private string ActorUserId => "System";
    private string ActorUserId =>
    User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

    [HttpPut("transition")]
    public async Task<IActionResult> Transition(Guid projectId, [FromBody] TransitionProjectStatusRequest request)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        try
        {
            await _history.TransitionAsync(projectId, request.ToProjectStatusId, ActorUserId, request.Note);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    
    [HttpGet("history")]
    public async Task<ActionResult<List<ProjectStatusHistoryResponse>>> GetHistory(Guid projectId)
    {
        var list = await _history.GetHistoryAsync(projectId);
        return Ok(list);
    }
}

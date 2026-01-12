using Microsoft.AspNetCore.Mvc;
using ToDo.API.Services;
using ToDo.Application.DTOs.Status;

namespace ToDo.API.Controllers;

[ApiController]
[Route("api/status-transitions")]
public class StatusTransitionsController : Controller
{
    private readonly IStatusService _statusService;

    public StatusTransitionsController(IStatusService statusService)
    {
        _statusService = statusService;
    }
    private string ActorUserId => User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "System";

    [HttpGet]
    [ProducesResponseType(typeof(List<StatusTransitionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _statusService.GetTransitionsAsync(ActorUserId);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateStatusTransitionRequest request)
    {
        if (request == null) return BadRequest();

        try
        {
            var id = await _statusService.AddTransitionAsync(request, ActorUserId);
            return Ok(id);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            await _statusService.RemoveTransitionAsync(id, ActorUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

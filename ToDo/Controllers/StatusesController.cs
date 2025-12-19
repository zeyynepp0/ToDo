using Microsoft.AspNetCore.Mvc;
using ToDo.API.Service;
using ToDo.API.Services;
using ToDo.Application.DTOs.Status;

namespace ToDo.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/statuses")]
public class StatusesController : Controller
{
    private readonly IStatusService _statusService;

    public StatusesController(IStatusService statusService)
    {
        _statusService = statusService;
    }

    private string ActorUserId => "System";// sonra silenecek jwt olmadığı için ekledik


    [HttpGet] // api/projects/{projectId:guid}/statuses
    // sırlaı olarak tüm statusları getirir.
    [ProducesResponseType(typeof(List<ProjectStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectStatuses([FromRoute] Guid projectId)
    {
        var list = await _statusService.GetProjectStatusesAsync(projectId);
        return Ok(list);
    }


    [HttpPut("reorder")]// api/projects/{projectId:guid}/statuses/reorder
    // statusları yeniden sıralar.
    public async Task<IActionResult> Reorder(
        [FromRoute] Guid projectId,
        [FromQuery] List<ReorderProjectStatusItem> items)
    {
        if (items is null || items.Count == 0)
            return BadRequest("Reorder list cannot be empty.");

        // orderNo 1..N gibi olsun istiyorsan kontrol:
        if (items.Any(x => x.OrderNo <= 0))
            return BadRequest("OrderNo must be greater than 0.");

        try
        {
            await _statusService.ReorderAsync(projectId, items, ActorUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            // status id’lerden biri bu projede yok
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // mantıksal hata
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{projectStatusId:guid}/toggle")] // api/projects/{projectId:guid}/statuses/{projectStatusId:guid}/toggle
    // statusu etkinleştirir veya devre dışı bırakır.
    public async Task<IActionResult> Toggle(
        [FromRoute] Guid projectId,
        [FromRoute] Guid projectStatusId,
        [FromQuery] ToggleProjectStatusRequest request)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        try
        {
            await _statusService.ToggleAsync(projectId, projectStatusId, request.IsEnabled, ActorUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    [HttpPost("custom")]// api/projects/{projectId:guid}/statuses/custom
                        // özel bir status ekler.
    public async Task<IActionResult> AddCustomStatus(
        [FromRoute] Guid projectId,
        [FromQuery] AddCustomStatusRequest request)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var id = await _statusService.AddCustomStatusAsync(projectId, request.Name.Trim(), ActorUserId);
        return Ok(id);
    }
}

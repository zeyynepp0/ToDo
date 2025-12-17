using Microsoft.AspNetCore.Mvc;
using ToDo.API.Services;
using ToDo.Application.DTOs;
using ToDo.Application.Services;


namespace ToDo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : Controller
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects)
    {
        _projects = projects;
    }

    private string ActorUserId => "System"; // jwt şuan eklemediğim için böyle
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateProjectRequest request)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");


        var id = await _projects.CreateProjectAsync(request, ActorUserId);
        return Ok(id);
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectSummaryResponse>>> GetAll()
    {
        var list = await _projects.GetAllProjectsAsync();
        return Ok(list);
    }

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> GetById(Guid projectId)
    {
        var project = await _projects.GetProjectByIdAsync(projectId);
        return project is null ? NotFound() : Ok(project);
    }

   
   
}

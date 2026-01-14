using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDo.API.Services;
using ToDo.Application.DTOs.Project;
using ToDo.Application.Services;
using System.Security.Claims;


namespace ToDo.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]


public class ProjectsController : Controller
{
    private readonly IProjectService _projects;
    private string ActorUserId =>
    User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

    public ProjectsController(IProjectService projects)
    {
        _projects = projects;
    }

  
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromQuery] CreateProjectRequest request)
    {

     

        var id = await _projects.CreateProjectAsync(request, ActorUserId);
        return Ok(id);
    }


   // [Authorize(Roles = "Admin")]
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

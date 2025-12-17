using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.DTOs;

namespace ToDo.Application.Services
{
    public interface IProjectService
    {
        Task<Guid> CreateProjectAsync(CreateProjectRequest request, string actorUserId);
        Task<List<ProjectSummaryResponse>> GetAllProjectsAsync();
        Task<ProjectDetailResponse?> GetProjectByIdAsync(Guid projectId);
    }
}

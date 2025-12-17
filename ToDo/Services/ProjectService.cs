using Microsoft.EntityFrameworkCore;
 using ToDo.Infrastructure.Contexts;
using ToDo.Application.DTOs;
using ToDo.Domain.Entities;

namespace ToDo.Application.Services
{
    public sealed class ProjectService : IProjectService
    {
        private readonly AppDbContext _db;

        public ProjectService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }


        //-----------------------CreateProjectAsync--------------------
        public async Task<Guid> CreateProjectAsync(CreateProjectRequest request, string actorUserId)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(actorUserId)) actorUserId = "System";

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false,

                CreatedByuserId = actorUserId // sonra düzenlenecek
            };

            _db.Projects.Add(project);

            var defs = await _db.StatusDefinitions
            .AsNoTracking()
            .Where(x => x.IsSystem && (x.SystemCode == "NEW" || x.SystemCode == "IN_PROGRESS" || x.SystemCode == "DONE"))
            .ToListAsync();

            var defNew = defs.Single(x => x.SystemCode == "NEW");
            var defProg = defs.Single(x => x.SystemCode == "IN_PROGRESS");
            var defDone = defs.Single(x => x.SystemCode == "DONE");


            var psNew = new ProjectStatus
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                StatusDefinitionId = defNew.Id,
                OrderNo = 1,
                IsEnabled = true,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false,
                CreatedByuserId = actorUserId,

            };

            var psProg = new ProjectStatus
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                StatusDefinitionId = defProg.Id,
                OrderNo = 2,
                IsEnabled = true,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false,
                CreatedByuserId = actorUserId
            };

            var psDone = new ProjectStatus
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                StatusDefinitionId = defDone.Id,
                OrderNo = 3,
                IsEnabled = true,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false,
                CreatedByuserId = actorUserId
            };

            _db.ProjectStatuses.AddRange(psNew, psProg, psDone);

            _db.ProjectStatusHistories.Add(new ProjectStatusHistory
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                FromProjectStatusId = null,
                ToProjectStatusId = psNew.Id,
                ChangedByUserId = actorUserId, 
                ChangedAt = DateTime.UtcNow,
                Note = "Project created -> initial status set"

            });

            await _db.SaveChangesAsync();
            return project.Id;
        }


        //------------ProjectSummaryResponse--------------------
        public async Task<List<ProjectSummaryResponse>> GetAllProjectsAsync()
        {
            return await _db.Projects
           .AsNoTracking()
           .OrderByDescending(x => x.CreatedDate)
           .Select(x => new ProjectSummaryResponse
           {
               Id = x.Id,
               Name = x.Name,
               Description = x.Description,
               CreatedDate = x.CreatedDate
           })
           .ToListAsync();
        }



        //-----------------ProjectDetailResponse--------------------
        public async Task<ProjectDetailResponse?> GetProjectByIdAsync(Guid projectId)
        {
            var project = await _db.Projects
           .AsNoTracking()
           .Where(x => x.Id == projectId)
           .Select(x => new ProjectDetailResponse
           {
               Id = x.Id,
               Name = x.Name,
               Description = x.Description,
               CreatedDate = x.CreatedDate
           })
           .FirstOrDefaultAsync();

            if (project is null) return null;

            // Statuses (order)
            project.Statuses = await _db.ProjectStatuses
                .AsNoTracking()
                .Where(ps => ps.ProjectId == projectId)
                .OrderBy(ps => ps.OrderNo)
                .Select(ps => new ProjectStatusResponse
                {
                    ProjectStatusId = ps.Id,
                    StatusDefinitionId = ps.StatusDefinitionId,
                    OrderNo = ps.OrderNo,
                    IsEnabled = ps.IsEnabled,
                    Name = ps.StatusDefinition.Name,
                    SystemCode = ps.StatusDefinition.SystemCode,
                    IsSystem = ps.StatusDefinition.IsSystem
                })
                .ToListAsync();

            return project;
        }
    }
    }


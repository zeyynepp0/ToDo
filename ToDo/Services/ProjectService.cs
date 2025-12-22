using Microsoft.EntityFrameworkCore;
using ToDo.Application.DTOs;
using ToDo.Application.DTOs.Project;
using ToDo.Application.DTOs.Status;
using ToDo.Application.DTOs.Tasks;
using ToDo.Domain.Entities;
 using ToDo.Infrastructure.Contexts;

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
            //if (string.IsNullOrWhiteSpace(actorUserId)) actorUserId = "System";

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

            // Statuses
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
                    IsSystem = ps.StatusDefinition.IsSystem,
                    Tasks = new List<TaskTreeResponse>() // boş başlat
                })
                .ToListAsync();

            // All tasks (flat)
            var allTasks = await _db.ProjectTasks
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.OrderNo)
                .Select(t => new TaskTreeResponse
                {
                    Id = t.Id,
                    ProjectStatusId = t.ProjectStatusId,
                    ParentTaskId = t.ParentTaskId ?? Guid.Empty,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    OrderNo = t.OrderNo,
                    Children = new List<TaskTreeResponse>()
                })
                .ToListAsync();

            // Build trees once -> Dictionary<StatusId, RootTasks>
            var treesByStatus = BuildTreesByStatus(allTasks);

            // Assign to statuses
            foreach (var st in project.Statuses)
            {
                st.Tasks = treesByStatus.TryGetValue(st.ProjectStatusId, out var roots)
                    ? roots
                    : new List<TaskTreeResponse>();
            }

            return project;
        }


        private static Dictionary<Guid, List<TaskTreeResponse>> BuildTreesByStatus(List<TaskTreeResponse> tasks)
        {
            if (tasks.Count == 0)
                return new Dictionary<Guid, List<TaskTreeResponse>>();

            // Id -> Task
            var byId = tasks.ToDictionary(t => t.Id);

            // her ihtimale karşı children reset
            foreach (var t in tasks)
                t.Children = new List<TaskTreeResponse>();

            //foreach (var t in tasks)
            //{
            //    if (t.ParentTaskId.HasValue &&
            //        byId.TryGetValue(t.ParentTaskId.Value, out var parent))
            //    {
            //        parent.Children.Add(t);
            //    }
            //}

            // With this corrected block:
            foreach (var t in tasks)
            {
                // Guid is a non-nullable value type, so check against Guid.Empty
                if (t.ParentTaskId != Guid.Empty &&
                    byId.TryGetValue(t.ParentTaskId, out var parent))
                {
                    parent.Children.Add(t);
                }
            }


            // parent-child bağla
            foreach (var t in tasks)
            {
                if (t.ParentTaskId is Guid parentId && byId.TryGetValue(parentId, out var parent))
                    parent.Children.Add(t);
            }



            // children listelerini OrderNo'ya göre sırala (tüm seviyelerde)
            foreach (var t in tasks)
                t.Children = t.Children.OrderBy(c => c.OrderNo).ToList();

            // root'ları status'a göre grupla
            return tasks
                .Where(t => t.ParentTaskId == Guid.Empty)
                .GroupBy(t => t.ProjectStatusId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.OrderNo).ToList()
                );
        }

    }
}


using Microsoft.EntityFrameworkCore;
using ToDo.Application.DTOs.Tasks;
using ToDo.Infrastructure.Contexts;
using ToDo.Domain.Entities;

namespace ToDo.API.Services;

public sealed class TaskService : ITaskService
{
    private readonly AppDbContext _db;

    public TaskService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> CreateAsync(Guid projectId, CreateTaskRequest request, string actorUserId)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Title)) throw new InvalidOperationException("Title is required.");
        actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;

        //  Status bu projeye ait mi
        var statusExists = await _db.ProjectStatuses
            .AnyAsync(s => s.Id == request.ProjectStatusId && s.ProjectId == projectId);

        if (!statusExists)
            throw new KeyNotFoundException("ProjectStatus not found for this project.");

        //  ParentTask varsa: aynı projede mi
        if (request.ParentTaskId.HasValue)
        {
            var parentOk = await _db.ProjectTasks.AnyAsync(t =>
                t.Id == request.ParentTaskId.Value &&
                t.ProjectId == projectId &&
                t.ProjectStatusId == request.ProjectStatusId);

            if (!parentOk)
                throw new InvalidOperationException("ParentTask must belong to same project and same status.");
        }

        // OrderNo null ise en sona
        var maxOrder = await _db.ProjectTasks
            .Where(t => t.ProjectId == projectId && t.ProjectStatusId == request.ProjectStatusId && t.ParentTaskId == request.ParentTaskId)
            .MaxAsync(t => (int?)t.OrderNo) ?? 0;

        var orderNo = request.OrderNo ?? (maxOrder + 1);

        var task = new ProjectTask
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ProjectStatusId = request.ProjectStatusId,
            ParentTaskId = request.ParentTaskId,

            Title = request.Title.Trim(),
            Description = request.Description,

            OrderNo = orderNo,
            IsCompleted = false,

            CreatedDate = DateTime.UtcNow,
            IsDeleted = false,


            CreatedByuserId = actorUserId
        };

        _db.ProjectTasks.Add(task);
        await _db.SaveChangesAsync();

        return task.Id;
    }

    public async Task<List<TaskTreeResponse>> GetTreeByStatusAsync(Guid projectId, Guid projectStatusId)
    {
        // o status bu projeye ait mi
        var statusExists = await _db.ProjectStatuses
            .AnyAsync(s => s.Id == projectStatusId && s.ProjectId == projectId);

        if (!statusExists)
            throw new KeyNotFoundException("ProjectStatus not found for this project.");

        // tek seferde çek N+1 olmasın
        var all = await _db.ProjectTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId && t.ProjectStatusId == projectStatusId)
            .OrderBy(t => t.OrderNo)
            .Select(t => new TaskTreeResponse
            {
                Id = t.Id,
                ProjectStatusId = t.ProjectStatusId,
                ParentTaskId = t.ParentTaskId,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                OrderNo = t.OrderNo
            })
            .ToListAsync();

        // Tree build
        var dict = all.ToDictionary(x => x.Id, x => x);
        var roots = new List<TaskTreeResponse>();

        foreach (var node in all)
        {
            if (node.ParentTaskId is null)
            {
                roots.Add(node);
                continue;
            }

            if (dict.TryGetValue(node.ParentTaskId.Value, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node); // parent bulunamazsa root say
        }

        // Children listelerini de orderNoya göre sırala
        SortTree(roots);
        return roots;
    }

    private static void SortTree(List<TaskTreeResponse> nodes)
    {
        nodes.Sort((a, b) => a.OrderNo.CompareTo(b.OrderNo));
        foreach (var n in nodes)
            SortTree(n.Children);
    }



    // ----------------MoveAsync --------------------
    public async Task MoveAsync(Guid projectId, Guid taskId, MoveTaskRequest request, string actorUserId)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;

        var task = _db.ProjectTasks
            .FirstOrDefault(t => t.Id == taskId && t.ProjectId == projectId);

        if (task is null)
            throw new KeyNotFoundException("Task not found for this project.");

        var statusExists = _db.ProjectStatuses
            .Any(s => s.Id == request.ToProjectStatusId && s.ProjectId == projectId);

        if (!statusExists)
            throw new KeyNotFoundException("Target ProjectStatus not found for this project.");

        if (task.ProjectStatusId == request.ToProjectStatusId)
            throw new InvalidOperationException("Task is already in this status. Use reorder endpoint.");

        var root = await _db.ProjectTasks
    .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);


        var allTasks = await _db.ProjectTasks
        .Where(t => t.ProjectId == projectId)
        .ToListAsync();

        var idsToMove = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(root.Id);


        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            if (!idsToMove.Add(currentId)) continue;

            foreach (var child in allTasks.Where(x => x.ParentTaskId == currentId))
                queue.Enqueue(child.Id);
        }

        var subtree = allTasks.Where(t => idsToMove.Contains(t.Id)).ToList();

        var maxOrder = await _db.ProjectTasks
        .Where(t => t.ProjectId == projectId &&
                    t.ProjectStatusId == request.ToProjectStatusId &&
                    t.ParentTaskId == null)
        .MaxAsync(t => (int?)t.OrderNo) ?? 0;

        root.OrderNo = request.OrderNo.HasValue && request.OrderNo.Value > 0
            ? request.OrderNo.Value
            : (maxOrder + 1);

        foreach (var t in subtree)
        {
            t.ProjectStatusId = request.ToProjectStatusId;
            t.UpdatedByUserId = actorUserId;
            t.UpdatedDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        //task.ParentTaskId = null;
        //task.ProjectStatusId = request.ToProjectStatusId;

        //var maxOrder = _db.ProjectTasks
        //    .Where(t => t.ProjectId == projectId &&
        //    t.ProjectStatusId == request.ToProjectStatusId &&
        //    t.ParentTaskId == null)
        //   .Max(t => (int?)t.OrderNo) ?? 0;

        //task.OrderNo = request.OrderNo.HasValue && request.OrderNo.Value > 0
        //   ? request.OrderNo.Value
        //   : (maxOrder + 1);

        //task.UpdatedByUserId = actorUserId;
        //task.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
   


    }

    //--------------------- ReorderASync --------------------
    public async Task ReorderAsync(Guid projectId, Guid projectStatusId, Guid? parentTaskId, List<ReorderTaskItem> items, string actorUserId)
    {
        if (items is null || items.Count == 0)
            throw new InvalidOperationException("Items are required.");

        actorUserId = string.IsNullOrWhiteSpace(actorUserId) ? "System" : actorUserId;

        
        if (items.Any(x => x.OrderNo <= 0))
            throw new InvalidOperationException("OrderNo must be greater than 0.");

        
        if (items.Select(x => x.OrderNo).Distinct().Count() != items.Count)
            throw new InvalidOperationException("OrderNo values must be unique.");

        var ids = items.Select(x => x.TaskId).Distinct().ToList();

        
        var tasks = await _db.ProjectTasks
            .Where(t => t.ProjectId == projectId &&
                        t.ProjectStatusId == projectStatusId &&
                        t.ParentTaskId == parentTaskId &&
                        ids.Contains(t.Id))
            .ToListAsync();

        if (tasks.Count != ids.Count)
            throw new InvalidOperationException("Some tasks do not belong to this project/status/parent.");

       
        foreach (var t in tasks)
        {
            var newOrder = items.First(x => x.TaskId == t.Id).OrderNo;
            t.OrderNo = newOrder;
            t.UpdatedByUserId = actorUserId;
            t.UpdatedDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

}

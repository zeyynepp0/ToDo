using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Tasks
{
    public sealed class TaskTreeResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectStatusId { get; set; }
        public Guid? ParentTaskId { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }

        public bool IsCompleted { get; set; }
        public int OrderNo { get; set; }

        public List<TaskTreeResponse> Children { get; set; } = new();
    }
}

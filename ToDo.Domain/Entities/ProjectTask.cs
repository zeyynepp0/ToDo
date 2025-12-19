using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Domain.Entities.Common;

namespace ToDo.Domain.Entities
{
    public class ProjectTask : BaseAuditEntity
    {
        public Guid ProjectId { get; set; }
        public Guid ProjectStatusId { get; set; }

        public Guid? ParentTaskId { get; set; } //  self FK

        public string Title { get; set; } = default!;
        public string? Description { get; set; }

        public bool IsCompleted { get; set; } = false;
        public string? CompletedByUserId { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int OrderNo { get; set; } 

        
        public Project Project { get; set; } = default!;
        public ProjectStatus ProjectStatus { get; set; } = default!;

        public ProjectTask? ParentTask { get; set; }
        public ICollection<ProjectTask> Children { get; set; } = new List<ProjectTask>();
    }
}

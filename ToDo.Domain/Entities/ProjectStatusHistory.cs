using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Domain.Entities
{
    public class ProjectStatusHistory
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public Guid? FromProjectStatusId { get; set; }
        public Guid ToProjectStatusId { get; set; }

        public string ChangedByUserId { get; set; } = default!;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string? Note { get; set; }
        public Project Project { get; set; } = default!;
        public ProjectStatus? FromProjectStatus { get; set; }
        public ProjectStatus ToProjectStatus { get; set; } = default!;
    }
}

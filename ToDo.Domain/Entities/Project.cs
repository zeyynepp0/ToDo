using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Domain.Entities.Common;

namespace ToDo.Domain.Entities
{
    public class Project : BaseAuditEntity
    {
        public string Name { get; set; }=default!;
        public string? Description { get; set; }=default!;
        public string? CompletedByUserId { get; set; }
        public DateTime? CompletedAtDate { get; set; }
        public ICollection<ProjectStatus> ProjectStatuses { get; set; }=new List<ProjectStatus>();
        public ICollection<ProjectStatusHistory> StatusHistory { get; set; } = new List<ProjectStatusHistory>();
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}

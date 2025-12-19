using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Project
{
    public sealed class ProjectSummaryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

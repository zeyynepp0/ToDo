using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs
{
    public sealed class CreateTaskRequest
    {
        public Guid ProjectStatusId { get; set; }

        public Guid? ParentTaskId { get; set; }

        public string Title { get; set; } = default!;
        public string? Description { get; set; }

        public int? OrderNo { get; set; } 
    }
}

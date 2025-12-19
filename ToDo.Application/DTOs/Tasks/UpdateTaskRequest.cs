using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Tasks
{
    public sealed class UpdateTaskRequest
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}

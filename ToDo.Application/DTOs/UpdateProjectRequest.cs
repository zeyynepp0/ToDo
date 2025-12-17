using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs
{
    public sealed class UpdateProjectRequest
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }
}

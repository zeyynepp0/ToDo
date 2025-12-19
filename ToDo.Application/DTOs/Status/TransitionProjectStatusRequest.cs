using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Status
{
    public sealed class TransitionProjectStatusRequest
    {
        public Guid ToProjectStatusId { get; set; }
        public string? Note { get; set; }
    }
}

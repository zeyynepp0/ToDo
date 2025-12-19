using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Status
{
    public sealed class ProjectStatusHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }

        public Guid? FromProjectStatusId { get; set; }
        public Guid ToProjectStatusId { get; set; }

        public string? Note { get; set; }
        public string ChangedByUserId { get; set; } = default!;
        public DateTime ChangedAt { get; set; }
    }
}

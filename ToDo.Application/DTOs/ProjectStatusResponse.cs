using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs
{
    //sealed bu sınıftan miraz alınamaz demektir
    public sealed class ProjectStatusResponse
    {
        public Guid ProjectStatusId { get; set; }
        public Guid StatusDefinitionId { get; set; }

        public string Name { get; set; } = default!;
        public string? SystemCode { get; set; }

        public bool IsSystem { get; set; }
        public bool IsEnabled { get; set; }

        public int OrderNo { get; set; }
    }
}

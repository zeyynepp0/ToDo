using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Domain.Entities.Common;

namespace ToDo.Domain.Entities
{// sabit durumlar ve kullanıcı tanımlı durumlar için
    public class StatusDefinition :BaseAuditEntity
    {
        public string Name { get; set; } = default!;
        public bool IsSystem { get; set; } = false; // sabit durumlar içim true
        public string? SystemCode { get; set; } // bunu bilemedim
        public bool IsActive { get; set; } = true;
       public ICollection<ProjectStatus> ProjectStatuses { get; set; } = new List<ProjectStatus>();
    }
}

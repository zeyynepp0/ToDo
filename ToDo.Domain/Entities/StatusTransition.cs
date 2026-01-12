using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Domain.Entities.Common;

namespace ToDo.Domain.Entities
{
    public class StatusTransition:BaseAuditEntity
    {
       
        public Guid? FromStatusDefinitionId { get; set; }
        public StatusDefinition? FromStatusDefinition { get; set; }

        // Hangi duruma
        public Guid ToStatusDefinitionId { get; set; }
        public StatusDefinition ToStatusDefinition { get; set; }

        // Bu kural aktif mi
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


    }
}

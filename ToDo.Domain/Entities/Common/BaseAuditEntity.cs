using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Domain.Entities.Common
{
    public abstract class BaseAuditEntity
    {
        public Guid Id { get; set; }
        public string CreatedByuserId { get; set; }=default!;
        public DateTime CreatedDate { get; set; }=DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedByUserId { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? DeletedByUserId { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsDeleted { get; set; }=false;
    }
}

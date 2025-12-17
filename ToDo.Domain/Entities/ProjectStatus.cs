using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Domain.Entities.Common;

namespace ToDo.Domain.Entities
{// bir projenin sahip olabileceği durumları ve sıralamasını tutar
    public class ProjectStatus : BaseAuditEntity
    {
        public Guid ProjectId { get; set; }
        public Guid StatusDefinitionId { get; set; }
        public int OrderNo { get; set; } // sıralamayı değiştirebilelim
        public bool IsEnabled { get; set; } = true;//aktif pasif
        public Project Project { get; set; } = default!;
        public StatusDefinition StatusDefinition { get; set; } = default!;

        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}

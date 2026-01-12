using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Status
{
    public class StatusTransitionResponse
    {
        public Guid Id { get; set; }

        public Guid? FromStatusDefinitionId { get; set; }
        public string? FromStatusName { get; set; } // UI'da göstermek için (örn: "Analiz")

        public Guid ToStatusDefinitionId { get; set; }
        public string ToStatusName { get; set; } // UI'da göstermek için (örn: "Geliştirme")

        public bool IsActive { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Status
{
    public class CreateStatusTransitionRequest
    {
        // Hangi statüden? (Null ise: İlk başlangıç/Create durumu)
        public Guid? FromStatusDefinitionId { get; set; }

        // Hangi statüye?
        public Guid ToStatusDefinitionId { get; set; }
    }
}

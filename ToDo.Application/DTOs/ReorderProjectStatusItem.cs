using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs
{
    public sealed class ReorderProjectStatusItem
    {
        public Guid ProjectStatusId { get; set; }
        public int OrderNo { get; set; }
    }
}

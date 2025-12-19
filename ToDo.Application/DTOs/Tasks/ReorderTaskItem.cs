using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Tasks
{
    public sealed class ReorderTaskItem
    {
        public Guid TaskId { get; set; }
        public int OrderNo { get; set; }  //1'den başlasın
    }
}

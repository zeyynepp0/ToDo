using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Tasks
{
    public sealed class MoveTaskRequest
    {
        public Guid ToProjectStatusId { get; set; } //hedef projectStatus.ID
        public int? OrderNo { get; set; } //null ise sona at
    }
}

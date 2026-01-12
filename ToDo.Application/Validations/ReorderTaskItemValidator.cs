using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.DTOs.Tasks;

namespace ToDo.Application.Validations
{
    public class ReorderTaskItemValidator : AbstractValidator<ReorderTaskItem>
    {
        public ReorderTaskItemValidator()
        {

            RuleFor(x => x.OrderNo)
                .GreaterThanOrEqualTo(0).WithMessage("Sıra numarası negatif olamaz.");
        }
    }
}

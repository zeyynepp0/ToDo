using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.DTOs.Tasks;

namespace ToDo.Application.Validations
{
    public class MoveTaskRequestValidator : AbstractValidator<MoveTaskRequest>
    {
        public MoveTaskRequestValidator()
        {
            RuleFor(x => x.ToProjectStatusId)
                .NotEmpty().WithMessage("Hedef proje statüsü belirtilmelidir.");

            // Sıra numarası verilmişse negatif olamaz
            RuleFor(x => x.OrderNo)
                .GreaterThanOrEqualTo(0).WithMessage("Sıra numarası 0'dan küçük olamaz.")
                .When(x => x.OrderNo.HasValue);
        }
    }
}


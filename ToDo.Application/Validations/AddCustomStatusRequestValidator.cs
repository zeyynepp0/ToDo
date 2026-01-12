using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.DTOs.Status;

namespace ToDo.Application.Validations
{
    public class AddCustomStatusRequestValidator : AbstractValidator<AddCustomStatusRequest>
    {
        public AddCustomStatusRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Statü adı boş bırakılamaz.")
                .MaximumLength(50).WithMessage("Statü adı en fazla 50 karakter olabilir.");
        }
    }
}

using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.DTOs.Status;

namespace ToDo.Application.Validations
{
    public class TransitionProjectStatusRequestValidator : AbstractValidator<TransitionProjectStatusRequest>
    {
        public TransitionProjectStatusRequestValidator()
        {
            RuleFor(x => x.ToProjectStatusId)
                .NotEmpty().WithMessage("Geçiş yapılacak statü ID'si zorunludur.");

            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("Not alanı en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Note));
        }
    }
}

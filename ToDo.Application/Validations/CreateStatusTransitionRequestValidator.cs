using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.DTOs.Status;

namespace ToDo.Application.Validations
{
    public class CreateStatusTransitionRequestValidator : AbstractValidator<CreateStatusTransitionRequest>
    {
        public CreateStatusTransitionRequestValidator()
        {
            // FromStatusDefinitionId null olabilir (başlangıç durumu), bu yüzden kontrol etmiyoruz.

            RuleFor(x => x.ToStatusDefinitionId)
                .NotEmpty().WithMessage("Hedef statü (ToStatus) seçilmelidir.");

            // Aynı statüye geçişi engellemek isterseniz:
            RuleFor(x => x)
                .Must(x => x.FromStatusDefinitionId != x.ToStatusDefinitionId)
                .WithMessage("Başlangıç ve hedef statü aynı olamaz.")
                .When(x => x.FromStatusDefinitionId.HasValue);
        }
    }
}

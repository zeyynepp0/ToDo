using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.DTOs.Tasks;

namespace ToDo.Application.Validations
{
    public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
    {
        public CreateTaskRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Görev başlığı zorunludur.")
                .MaximumLength(200).WithMessage("Görev başlığı 200 karakteri geçemez.");

            RuleFor(x => x.ProjectStatusId)
                .NotEmpty().WithMessage("Proje durum ID'si (Status) zorunludur.");

            // Eğer OrderNo girildiyse (nullable olduğu için When kullandık veya null kontrolü otomatik yapılır)
            RuleFor(x => x.OrderNo)
                .GreaterThanOrEqualTo(0).WithMessage("Sıra numarası negatif olamaz.")
                .When(x => x.OrderNo.HasValue);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Açıklama çok uzun.");
        }
    }
}

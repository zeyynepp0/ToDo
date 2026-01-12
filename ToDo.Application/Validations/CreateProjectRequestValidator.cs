using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.DTOs.Project;

namespace ToDo.Application.Validations
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Proje adı boş bırakılamaz.")
                .MaximumLength(100).WithMessage("Proje adı 100 karakterden uzun olamaz.")
                .MinimumLength(3).WithMessage("Proje adı en az 3 karakterden uzun olmalı.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama 500 karakterden uzun olamaz.");
        }
    }
}

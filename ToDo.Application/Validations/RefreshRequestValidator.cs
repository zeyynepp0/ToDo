using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.DTOs.Auth;

namespace ToDo.Application.Validations
{
    public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
    {
        public RefreshRequestValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access Token gereklidir.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh Token gereklidir.");
        }
    }
}

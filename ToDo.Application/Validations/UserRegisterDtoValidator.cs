using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using ToDo.Application.DTOs;

namespace ToDo.Application.Validations
{
   public class UserRegisterDtoValidator: AbstractValidator<UserRegisterDto>
    {
        public UserRegisterDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ad alanı boş geçilemez.")
                .MaximumLength(50).WithMessage("Ad 50 karakterden uzun olamaz.");

        RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyad alanı boş geçilemez.")
                .MaximumLength(50).WithMessage("Soyad 50 karakterden uzun olamaz.");

        RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi gereklidir.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
                .Matches(@"[!@#$%^&*(),.?""{}|<>]").WithMessage("Şifre en az bir özel karakter içermelidir.");


            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Telefon numarası çok uzun.")
                .Matches(@"^\+?\d{10,15}$").WithMessage("Telefon numarasınıdoğru formatta giriniz.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
}

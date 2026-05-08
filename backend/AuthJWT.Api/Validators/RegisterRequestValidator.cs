using AuthJWT.Api.DTOs;
using FluentValidation;

namespace AuthJWT.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.");

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("E-mail inválido.");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.");
    }
}

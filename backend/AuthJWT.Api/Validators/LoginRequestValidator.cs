using AuthJWT.Api.DTOs;
using FluentValidation;

namespace AuthJWT.Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("E-mail inválido.");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}

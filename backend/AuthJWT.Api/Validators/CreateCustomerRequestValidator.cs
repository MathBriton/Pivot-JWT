using AuthJWT.Api.DTOs;
using FluentValidation;

namespace AuthJWT.Api.Validators;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("E-mail inválido.");
    }
}

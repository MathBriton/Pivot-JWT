using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using FluentValidation;

namespace AuthJWT.Api.Validators;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("E-mail inválido.");

        RuleFor(r => r.Status)
            .NotEmpty().WithMessage("Status é obrigatório.")
            .Must(s => CustomerStatus.All.Contains(s))
            .WithMessage($"Status deve ser: {string.Join(" ou ", CustomerStatus.All)}.");
    }
}

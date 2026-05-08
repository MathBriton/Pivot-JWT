using AuthJWT.Api.DTOs;
using FluentValidation;

namespace AuthJWT.Api.Validators;

public class CreateWorkTaskRequestValidator : AbstractValidator<CreateWorkTaskRequest>
{
    public CreateWorkTaskRequestValidator()
    {
        RuleFor(r => r.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(300).WithMessage("Título deve ter no máximo 300 caracteres.");

        RuleFor(r => r.Deadline)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Prazo deve ser uma data futura.")
            .When(r => r.Deadline.HasValue);
    }
}

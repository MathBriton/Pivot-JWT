using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using FluentValidation;

namespace AuthJWT.Api.Validators;

public class ChangeWorkTaskStatusRequestValidator : AbstractValidator<ChangeWorkTaskStatusRequest>
{
    public ChangeWorkTaskStatusRequestValidator()
    {
        RuleFor(r => r.Status)
            .NotEmpty().WithMessage("Status é obrigatório.")
            .Must(s => WorkTaskStatus.All.Contains(s))
            .WithMessage($"Status deve ser um de: {string.Join(", ", WorkTaskStatus.All)}.");
    }
}

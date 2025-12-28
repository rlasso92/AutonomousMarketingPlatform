using AutonomousMarketingPlatform.Application.DTOs;
using FluentValidation;

namespace AutonomousMarketingPlatform.Application.Validators;

/// <summary>
/// Validador para CreateCampaignDto.
/// </summary>
public class CreateCampaignDtoValidator : AbstractValidator<CreateCampaignDto>
{
    public CreateCampaignDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la campaña es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.");

        RuleFor(x => x.Status)
            .Must(status => new[] { "Draft", "Active", "Paused", "Archived" }.Contains(status))
            .WithMessage("El estado debe ser: Draft, Active, Paused o Archived.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Budget.HasValue)
            .WithMessage("El presupuesto no puede ser negativo.");
    }
}

/// <summary>
/// Validador para UpdateCampaignDto.
/// </summary>
public class UpdateCampaignDtoValidator : AbstractValidator<UpdateCampaignDto>
{
    public UpdateCampaignDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la campaña es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.");

        RuleFor(x => x.Status)
            .Must(status => new[] { "Draft", "Active", "Paused", "Archived" }.Contains(status))
            .WithMessage("El estado debe ser: Draft, Active, Paused o Archived.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Budget.HasValue)
            .WithMessage("El presupuesto no puede ser negativo.");
    }
}


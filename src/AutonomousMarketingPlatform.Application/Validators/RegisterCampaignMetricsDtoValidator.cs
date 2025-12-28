using AutonomousMarketingPlatform.Application.DTOs;
using FluentValidation;

namespace AutonomousMarketingPlatform.Application.Validators;

/// <summary>
/// Validador para RegisterCampaignMetricsDto.
/// </summary>
public class RegisterCampaignMetricsDtoValidator : AbstractValidator<RegisterCampaignMetricsDto>
{
    public RegisterCampaignMetricsDtoValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("El ID de la campaña es obligatorio.");

        RuleFor(x => x.MetricDate)
            .NotEmpty().WithMessage("La fecha de las métricas es obligatoria.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.Impressions)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Las impresiones no pueden ser negativas.");

        RuleFor(x => x.Clicks)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los clics no pueden ser negativos.");

        RuleFor(x => x.Likes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los likes no pueden ser negativos.");

        RuleFor(x => x.Comments)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los comentarios no pueden ser negativos.");

        RuleFor(x => x.Shares)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los compartidos no pueden ser negativos.");

        RuleFor(x => x.Source)
            .MaximumLength(50)
            .WithMessage("La fuente no puede exceder 50 caracteres.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Las notas no pueden exceder 2000 caracteres.");
    }
}

/// <summary>
/// Validador para RegisterPublishingJobMetricsDto.
/// </summary>
public class RegisterPublishingJobMetricsDtoValidator : AbstractValidator<RegisterPublishingJobMetricsDto>
{
    public RegisterPublishingJobMetricsDtoValidator()
    {
        RuleFor(x => x.PublishingJobId)
            .NotEmpty().WithMessage("El ID de la publicación es obligatorio.");

        RuleFor(x => x.MetricDate)
            .NotEmpty().WithMessage("La fecha de las métricas es obligatoria.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.Impressions)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Las impresiones no pueden ser negativas.");

        RuleFor(x => x.Clicks)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los clics no pueden ser negativos.");

        RuleFor(x => x.Likes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los likes no pueden ser negativos.");

        RuleFor(x => x.Comments)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los comentarios no pueden ser negativos.");

        RuleFor(x => x.Shares)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los compartidos no pueden ser negativos.");

        RuleFor(x => x.Source)
            .MaximumLength(50)
            .WithMessage("La fuente no puede exceder 50 caracteres.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Las notas no pueden exceder 2000 caracteres.");
    }
}


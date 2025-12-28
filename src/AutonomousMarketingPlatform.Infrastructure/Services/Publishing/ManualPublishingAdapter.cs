using AutonomousMarketingPlatform.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Infrastructure.Services.Publishing;

/// <summary>
/// Adaptador para publicación manual (Fase A).
/// Genera paquetes listos para descargar y publicar manualmente.
/// </summary>
public class ManualPublishingAdapter : IPublishingAdapter
{
    private readonly ILogger<ManualPublishingAdapter> _logger;

    public string ChannelName => "Manual";

    public ManualPublishingAdapter(ILogger<ManualPublishingAdapter> logger)
    {
        _logger = logger;
    }

    public Task<bool> CanPublishAsync(CancellationToken cancellationToken = default)
    {
        // Siempre disponible para publicación manual
        return Task.FromResult(true);
    }

    public Task<PublishingPackage> GeneratePackageAsync(
        PublishingPayload payload,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generando paquete manual para publicación");

        var package = new PublishingPackage
        {
            Copy = payload.Copy,
            Hashtags = payload.Hashtags,
            MediaUrls = payload.MediaUrls,
            Checklist = new Dictionary<string, string>
            {
                { "Copy", "✓ Copiar el texto del copy" },
                { "Hashtags", "✓ Agregar los hashtags sugeridos" },
                { "Media", "✓ Subir las imágenes/videos" },
                { "Schedule", "✓ Programar o publicar inmediatamente" },
                { "Review", "✓ Revisar antes de publicar" }
            },
            Metadata = new Dictionary<string, object>
            {
                { "CampaignId", payload.CampaignId ?? "" },
                { "MarketingPackId", payload.MarketingPackId ?? "" },
                { "GeneratedAt", DateTime.UtcNow },
                { "Format", "Manual" }
            }
        };

        return Task.FromResult(package);
    }

    public Task<PublishingResult> PublishAsync(
        PublishingPayload payload,
        DateTime? scheduledDate,
        CancellationToken cancellationToken = default)
    {
        // Para publicación manual, no publicamos realmente
        // El resultado se marca como RequiresApproval
        _logger.LogInformation("Publicación manual requiere aprobación");

        return Task.FromResult(new PublishingResult
        {
            Success = false,
            ErrorMessage = "Requires manual approval",
            IsTransientError = false
        });
    }
}


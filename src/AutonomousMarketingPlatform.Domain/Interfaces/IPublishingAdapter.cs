namespace AutonomousMarketingPlatform.Domain.Interfaces;

/// <summary>
/// Interfaz para adaptadores de publicación a redes sociales.
/// Permite diferentes implementaciones por canal (Manual, Facebook, Instagram, TikTok).
/// </summary>
public interface IPublishingAdapter
{
    /// <summary>
    /// Nombre del canal que maneja este adaptador.
    /// </summary>
    string ChannelName { get; }

    /// <summary>
    /// Publica contenido en la red social correspondiente.
    /// </summary>
    /// <param name="payload">Datos de la publicación (copy, media, hashtags, etc.)</param>
    /// <param name="scheduledDate">Fecha programada (null para publicación inmediata)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la publicación con URL y metadata</returns>
    Task<PublishingResult> PublishAsync(
        PublishingPayload payload,
        DateTime? scheduledDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si el adaptador puede publicar (tokens válidos, permisos, etc.).
    /// </summary>
    Task<bool> CanPublishAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un paquete listo para publicación manual (Fase A).
    /// </summary>
    Task<PublishingPackage> GeneratePackageAsync(
        PublishingPayload payload,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Payload de publicación con toda la información necesaria.
/// </summary>
public class PublishingPayload
{
    public string Copy { get; set; } = string.Empty;
    public string? Hashtags { get; set; }
    public List<string> MediaUrls { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
    public string? CampaignId { get; set; }
    public string? MarketingPackId { get; set; }
}

/// <summary>
/// Resultado de una publicación.
/// </summary>
public class PublishingResult
{
    public bool Success { get; set; }
    public string? PublishedUrl { get; set; }
    public string? ExternalPostId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsTransientError { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Paquete listo para publicación manual (Fase A).
/// </summary>
public class PublishingPackage
{
    public string Copy { get; set; } = string.Empty;
    public string? Hashtags { get; set; }
    public List<string> MediaUrls { get; set; } = new();
    public Dictionary<string, string> Checklist { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string Format { get; set; } = "JSON"; // JSON, ZIP
}


using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities;

/// <summary>
/// Representa un trabajo de publicación en redes sociales.
/// Puede ser una publicación programada o ya publicada.
/// </summary>
public class PublishingJob : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Identificador del tenant.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Identificador de la campaña asociada.
    /// </summary>
    public Guid CampaignId { get; set; }

    /// <summary>
    /// Identificador del MarketingPack usado para esta publicación.
    /// </summary>
    public Guid? MarketingPackId { get; set; }

    /// <summary>
    /// Identificador del GeneratedCopy usado.
    /// </summary>
    public Guid? GeneratedCopyId { get; set; }

    /// <summary>
    /// Identificador del MarketingAssetPrompt usado (si aplica).
    /// </summary>
    public Guid? MarketingAssetPromptId { get; set; }

    /// <summary>
    /// Canal de publicación (Instagram, Facebook, TikTok, etc.).
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Estado del trabajo (Pending, Processing, Success, Failed, RequiresApproval, Cancelled).
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Fecha programada para publicación.
    /// </summary>
    public DateTime? ScheduledDate { get; set; }
    
    /// <summary>
    /// Fecha en que se procesó el job.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Número máximo de reintentos permitidos.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Payload completo en JSON (copy, media refs, hashtags, etc.).
    /// </summary>
    public string? Payload { get; set; }
    
    /// <summary>
    /// URL para descargar el paquete listo para publicar (Fase A).
    /// </summary>
    public string? DownloadUrl { get; set; }
    
    /// <summary>
    /// Indica si requiere aprobación manual antes de publicar.
    /// </summary>
    public bool RequiresApproval { get; set; } = true;
    
    /// <summary>
    /// Fecha de aprobación (si aplica).
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// Usuario que aprobó la publicación.
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Fecha en que se publicó realmente.
    /// </summary>
    public DateTime? PublishedDate { get; set; }

    /// <summary>
    /// URL de la publicación en la red social.
    /// </summary>
    public string? PublishedUrl { get; set; }

    /// <summary>
    /// ID de la publicación en la red social.
    /// </summary>
    public string? ExternalPostId { get; set; }

    /// <summary>
    /// Contenido del post (copy).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Hashtags usados.
    /// </summary>
    public string? Hashtags { get; set; }

    /// <summary>
    /// URL de la imagen/video publicada.
    /// </summary>
    public string? MediaUrl { get; set; }

    /// <summary>
    /// Mensaje de error si falló la publicación.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Número de reintentos realizados.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Metadatos adicionales en JSON.
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation properties
    public virtual Campaign Campaign { get; set; } = null!;
    public virtual MarketingPack? MarketingPack { get; set; }
    public virtual GeneratedCopy? GeneratedCopy { get; set; }
    public virtual MarketingAssetPrompt? MarketingAssetPrompt { get; set; }
}


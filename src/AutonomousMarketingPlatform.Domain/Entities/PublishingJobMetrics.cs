using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities;

/// <summary>
/// Representa métricas de una publicación individual (post) en una fecha específica.
/// Permite tracking detallado por publicación.
/// </summary>
public class PublishingJobMetrics : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Identificador del tenant.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Identificador del PublishingJob (publicación).
    /// </summary>
    public Guid PublishingJobId { get; set; }

    /// <summary>
    /// Fecha de las métricas (solo fecha, sin hora).
    /// </summary>
    public DateTime MetricDate { get; set; }

    /// <summary>
    /// Número de impresiones (vistas) de esta publicación.
    /// </summary>
    public long Impressions { get; set; }

    /// <summary>
    /// Número de clics en esta publicación.
    /// </summary>
    public long Clicks { get; set; }

    /// <summary>
    /// Engagement total (likes + comments + shares).
    /// </summary>
    public long Engagement { get; set; }

    /// <summary>
    /// Desglose de engagement: likes.
    /// </summary>
    public long Likes { get; set; }

    /// <summary>
    /// Desglose de engagement: comentarios.
    /// </summary>
    public long Comments { get; set; }

    /// <summary>
    /// Desglose de engagement: compartidos.
    /// </summary>
    public long Shares { get; set; }

    /// <summary>
    /// Tasa de clics (CTR) calculada: (Clicks / Impressions) * 100.
    /// </summary>
    public decimal? ClickThroughRate { get; set; }

    /// <summary>
    /// Tasa de engagement calculada: (Engagement / Impressions) * 100.
    /// </summary>
    public decimal? EngagementRate { get; set; }

    /// <summary>
    /// Indica si las métricas fueron ingresadas manualmente.
    /// </summary>
    public bool IsManualEntry { get; set; }

    /// <summary>
    /// Fuente de las métricas (API, Manual, etc.).
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Notas adicionales sobre las métricas.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Metadatos adicionales en JSON.
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation properties
    public virtual PublishingJob PublishingJob { get; set; } = null!;
}


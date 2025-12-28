using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities;

/// <summary>
/// Representa métricas agregadas de una campaña en una fecha específica.
/// Permite tracking de rendimiento por día.
/// </summary>
public class CampaignMetrics : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Identificador del tenant.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Identificador de la campaña.
    /// </summary>
    public Guid CampaignId { get; set; }

    /// <summary>
    /// Fecha de las métricas (solo fecha, sin hora).
    /// </summary>
    public DateTime MetricDate { get; set; }

    /// <summary>
    /// Número de impresiones (vistas).
    /// </summary>
    public long Impressions { get; set; }

    /// <summary>
    /// Número de clics.
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
    /// Número de publicaciones activas en esta fecha.
    /// </summary>
    public int ActivePosts { get; set; }

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
    public virtual Campaign Campaign { get; set; } = null!;
}


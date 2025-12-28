using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities;

/// <summary>
/// Representa una campaña de marketing generada por el sistema.
/// </summary>
public class Campaign : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Identificador del tenant al que pertenece la campaña.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Nombre de la campaña.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la campaña.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Estado de la campaña (Draft, Active, Paused, Archived).
    /// </summary>
    public string Status { get; set; } = "Draft";
    
    /// <summary>
    /// Fecha de inicio de la campaña.
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Fecha de fin de la campaña.
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Presupuesto asignado a la campaña (opcional).
    /// </summary>
    public decimal? Budget { get; set; }
    
    /// <summary>
    /// Presupuesto gastado hasta el momento.
    /// </summary>
    public decimal? SpentAmount { get; set; }
    
    /// <summary>
    /// Objetivos de la campaña (JSON).
    /// </summary>
    public string? Objectives { get; set; }
    
    /// <summary>
    /// Audiencia objetivo (JSON).
    /// </summary>
    public string? TargetAudience { get; set; }
    
    /// <summary>
    /// Canales donde se publicará (JSON array).
    /// </summary>
    public string? TargetChannels { get; set; }
    
    /// <summary>
    /// Notas internas sobre la campaña.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Estrategia de marketing generada por IA.
    /// </summary>
    public string? MarketingStrategy { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();
    public virtual ICollection<MarketingPack> MarketingPacks { get; set; } = new List<MarketingPack>();
    public virtual ICollection<PublishingJob> PublishingJobs { get; set; } = new List<PublishingJob>();
    public virtual ICollection<MarketingMemory> MarketingMemories { get; set; } = new List<MarketingMemory>();
}


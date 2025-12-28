using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities;

/// <summary>
/// Relación entre Usuario y Tenant con un rol específico.
/// Permite que un usuario pertenezca a múltiples tenants con diferentes roles.
/// </summary>
public class UserTenant : BaseEntity
{
    /// <summary>
    /// Identificador del usuario.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identificador del tenant.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Identificador del rol del usuario en este tenant.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Indica si este es el tenant principal del usuario.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Fecha en que el usuario se unió a este tenant.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ApplicationRole Role { get; set; } = null!;
}



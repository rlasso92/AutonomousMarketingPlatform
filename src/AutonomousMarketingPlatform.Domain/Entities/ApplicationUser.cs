using Microsoft.AspNetCore.Identity;

namespace AutonomousMarketingPlatform.Domain.Entities;

/// <summary>
/// Usuario de la aplicación con soporte multi-tenant.
/// Extiende IdentityUser para usar ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Identificador del tenant principal del usuario.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el usuario está activo.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Número de intentos fallidos de login.
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Fecha hasta la cual el usuario está bloqueado (si aplica).
    /// </summary>
    public DateTime? LockoutEndDate { get; set; }

    /// <summary>
    /// Fecha del último inicio de sesión exitoso.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// IP del último inicio de sesión.
    /// </summary>
    public string? LastLoginIp { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
}



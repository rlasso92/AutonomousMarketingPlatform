using Microsoft.AspNetCore.Identity;

namespace AutonomousMarketingPlatform.Domain.Entities;

/// <summary>
/// Rol de la aplicación.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Descripción del rol.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indica si el rol está activo.
    /// </summary>
    public bool IsActive { get; set; } = true;
}



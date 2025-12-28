using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities;

/// <summary>
/// Entidad para registrar eventos de auditoría.
/// </summary>
public class AuditLog : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, etc.
    public string EntityType { get; set; } = string.Empty; // Campaign, Content, User, etc.
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; } // JSON con valores anteriores
    public string? NewValues { get; set; } // JSON con valores nuevos
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Result { get; set; } = string.Empty; // Success, Failed
    public string? ErrorMessage { get; set; }
    public string? RequestId { get; set; } // Para correlación con logs
    public string? AdditionalData { get; set; } // Datos adicionales en JSON
}

namespace AutonomousMarketingPlatform.Application.DTOs;

/// <summary>
/// DTO para lista de logs de aplicación.
/// </summary>
public class ApplicationLogListDto
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Path { get; set; }
    public string? HttpMethod { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para detalles completos de un log de aplicación.
/// </summary>
public class ApplicationLogDto
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? StackTrace { get; set; }
    public string? ExceptionType { get; set; }
    public string? InnerException { get; set; }
    public string? RequestId { get; set; }
    public string? Path { get; set; }
    public string? HttpMethod { get; set; }
    public string? AdditionalData { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}


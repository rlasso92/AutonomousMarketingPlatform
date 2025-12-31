namespace AutonomousMarketingPlatform.Application.DTOs;

/// <summary>
/// DTO para transferir información de consentimiento.
/// </summary>
public class ConsentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public string ConsentTypeDisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsGranted { get; set; }
    public DateTime? GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ConsentVersion { get; set; }
    public bool IsRequired { get; set; }
    public bool CanRevoke { get; set; }
}

/// <summary>
/// DTO para crear o actualizar un consentimiento.
/// </summary>
public class CreateConsentDto
{
    public string ConsentType { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public string? ConsentVersion { get; set; }
    /// <summary>
    /// ID del usuario al que se otorga el consentimiento (opcional, solo para super admin).
    /// Si no se especifica, se otorga al usuario autenticado.
    /// </summary>
    public Guid? TargetUserId { get; set; }
}

/// <summary>
/// DTO para la respuesta de consentimientos del usuario.
/// </summary>
public class UserConsentsDto
{
    public Guid UserId { get; set; }
    public List<ConsentDto> Consents { get; set; } = new();
    public bool AllRequiredConsentsGranted { get; set; }
    public List<string> MissingRequiredConsents { get; set; } = new();
}


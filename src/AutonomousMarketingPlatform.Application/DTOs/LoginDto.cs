namespace AutonomousMarketingPlatform.Application.DTOs;

/// <summary>
/// DTO para login.
/// </summary>
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid? TenantId { get; set; } // Opcional, puede venir del header
    public bool RememberMe { get; set; }
}

/// <summary>
/// DTO para resultado de login.
/// </summary>
public class LoginResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsLockedOut { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public int RemainingAttempts { get; set; }
}



namespace AutonomousMarketingPlatform.Application.DTOs;

/// <summary>
/// DTO para información de usuario.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public bool IsActive { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndDate { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear usuario.
/// </summary>
public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Role { get; set; } = "Marketer";
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO para actualizar usuario.
/// </summary>
public class UpdateUserDto
{
    public string? FullName { get; set; }
    public bool? IsActive { get; set; }
    public string? Role { get; set; }
}

/// <summary>
/// DTO para lista de usuarios.
/// </summary>
public class UserListDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// DTO para información de tenant.
/// </summary>
public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
}

/// <summary>
/// DTO para crear tenant.
/// </summary>
public class CreateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
    public string? ContactEmail { get; set; }
}

/// <summary>
/// DTO para actualizar tenant.
/// </summary>
public class UpdateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
}


using AutonomousMarketingPlatform.Application.DTOs;

namespace AutonomousMarketingPlatform.Application.Services;

/// <summary>
/// Interfaz para acceder a los logs de aplicación.
/// </summary>
public interface IApplicationLogRepository
{
    Task<List<ApplicationLogListDto>> GetLogsAsync(
        Guid? tenantId,
        Guid? userId,
        string? level,
        string? source,
        DateTime? fromDate,
        DateTime? toDate,
        int skip,
        int take,
        bool isSuperAdmin,
        CancellationToken cancellationToken = default);

    Task<ApplicationLogDto?> GetLogByIdAsync(
        Guid id,
        Guid? tenantId,
        bool isSuperAdmin,
        CancellationToken cancellationToken = default);
}


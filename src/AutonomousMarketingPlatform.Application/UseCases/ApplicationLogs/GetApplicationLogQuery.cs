using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using MediatR;

namespace AutonomousMarketingPlatform.Application.UseCases.ApplicationLogs;

/// <summary>
/// Query para obtener un log de aplicación por ID.
/// </summary>
public class GetApplicationLogQuery : IRequest<ApplicationLogDto?>
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public bool IsSuperAdmin { get; set; } = false;
}

/// <summary>
/// Handler para obtener un log de aplicación.
/// </summary>
public class GetApplicationLogQueryHandler : IRequestHandler<GetApplicationLogQuery, ApplicationLogDto?>
{
    private readonly IApplicationLogRepository _logRepository;

    public GetApplicationLogQueryHandler(IApplicationLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<ApplicationLogDto?> Handle(GetApplicationLogQuery request, CancellationToken cancellationToken)
    {
        return await _logRepository.GetLogByIdAsync(
            request.Id,
            request.TenantId,
            request.IsSuperAdmin,
            cancellationToken);
    }
}


using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using MediatR;

namespace AutonomousMarketingPlatform.Application.UseCases.ApplicationLogs;

/// <summary>
/// Query para listar logs de aplicación con filtros.
/// </summary>
public class ListApplicationLogsQuery : IRequest<List<ApplicationLogListDto>>
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string? Level { get; set; }
    public string? Source { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 100;
    public bool IsSuperAdmin { get; set; } = false;
}

/// <summary>
/// Handler para listar logs de aplicación.
/// </summary>
public class ListApplicationLogsQueryHandler : IRequestHandler<ListApplicationLogsQuery, List<ApplicationLogListDto>>
{
    private readonly IApplicationLogRepository _logRepository;

    public ListApplicationLogsQueryHandler(IApplicationLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<List<ApplicationLogListDto>> Handle(ListApplicationLogsQuery request, CancellationToken cancellationToken)
    {
        return await _logRepository.GetLogsAsync(
            request.TenantId,
            request.UserId,
            request.Level,
            request.Source,
            request.FromDate,
            request.ToDate,
            request.Skip,
            request.Take,
            request.IsSuperAdmin,
            cancellationToken);
    }
}


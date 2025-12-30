using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Campaigns;

/// <summary>
/// Query para listar campañas con filtros opcionales.
/// </summary>
public class ListCampaignsQuery : IRequest<List<CampaignListDto>>
{
    public Guid TenantId { get; set; }
    public string? Status { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
    public bool IsSuperAdmin { get; set; } = false;
}

/// <summary>
/// Handler para listar campañas.
/// </summary>
public class ListCampaignsQueryHandler : IRequestHandler<ListCampaignsQuery, List<CampaignListDto>>
{
    private readonly IRepository<Domain.Entities.Campaign> _campaignRepository;
    private readonly ILogger<ListCampaignsQueryHandler> _logger;

    public ListCampaignsQueryHandler(
        IRepository<Domain.Entities.Campaign> campaignRepository,
        ILogger<ListCampaignsQueryHandler> logger)
    {
        _campaignRepository = campaignRepository;
        _logger = logger;
    }

    public async Task<List<CampaignListDto>> Handle(ListCampaignsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando campañas: TenantId={TenantId}, Status={Status}, Skip={Skip}, Take={Take}, IsSuperAdmin={IsSuperAdmin}", 
            request.TenantId, request.Status ?? "Todos", request.Skip, request.Take, request.IsSuperAdmin);

        // Si es SuperAdmin y TenantId es Guid.Empty, mostrar todas las campañas
        // Si no, filtrar por TenantId
        // Nota: BaseRepository ya aplica IsActive, así que el predicado solo necesita el filtro de Status
        System.Linq.Expressions.Expression<Func<Domain.Entities.Campaign, bool>> predicate;
        
        if (request.IsSuperAdmin && request.TenantId == Guid.Empty)
        {
            // SuperAdmin sin tenant seleccionado: mostrar todas las campañas activas
            predicate = c => string.IsNullOrEmpty(request.Status) || c.Status == request.Status;
        }
        else
        {
            // Filtrar por TenantId (BaseRepository ya aplica IsActive y TenantId)
            predicate = c => string.IsNullOrEmpty(request.Status) || c.Status == request.Status;
        }

        // Para SuperAdmin con Guid.Empty, pasar Guid.Empty al repository para que no filtre por tenant
        var tenantIdForRepository = (request.IsSuperAdmin && request.TenantId == Guid.Empty) 
            ? Guid.Empty 
            : request.TenantId;

        var campaigns = await _campaignRepository.FindAsync(
            predicate,
            tenantIdForRepository,
            cancellationToken);

        _logger.LogInformation("Se encontraron {Count} campañas antes de filtrar por Status", campaigns?.Count() ?? 0);

        var result = campaigns
            .OrderByDescending(c => c.CreatedAt)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(c => new CampaignListDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ContentCount = c.Contents.Count,
                MarketingPackCount = c.MarketingPacks.Count,
                PublishingJobCount = c.PublishingJobs.Count,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                TenantId = c.TenantId
            })
            .ToList();

        _logger.LogInformation("Retornando {Count} campañas después de aplicar paginación", result.Count);

        return result;
    }
}


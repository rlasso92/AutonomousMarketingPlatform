using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using AutonomousMarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Infrastructure.Repositories;

/// <summary>
/// Repositorio específico para Campaigns con métodos adicionales.
/// </summary>
public class CampaignRepository : BaseRepository<Campaign>, ICampaignRepository
{
    public CampaignRepository(ApplicationDbContext context, ITenantService tenantService)
        : base(context, tenantService)
    {
    }

    public async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.TenantId == tenantId && 
                       c.Status == "Active" && 
                       c.IsActive)
            .Include(c => c.Contents)
            .ToListAsync(cancellationToken);
    }

    public async Task<Campaign?> GetCampaignWithDetailsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Id == id && c.TenantId == tenantId && c.IsActive)
            .Include(c => c.Contents)
            .Include(c => c.MarketingPacks)
                .ThenInclude(mp => mp.Copies)
            .Include(c => c.MarketingPacks)
                .ThenInclude(mp => mp.AssetPrompts)
            .Include(c => c.PublishingJobs)
            .Include(c => c.MarketingMemories)
            .FirstOrDefaultAsync(cancellationToken);
    }
}


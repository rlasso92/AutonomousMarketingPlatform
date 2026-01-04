using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AutonomousMarketingPlatform.Application.UseCases.Tenants;

/// <summary>
/// Query para obtener un tenant por ID.
/// </summary>
public class GetTenantQuery : IRequest<TenantDto?>
{
    public Guid TenantId { get; set; }
}

/// <summary>
/// Handler para obtener tenant.
/// </summary>
public class GetTenantQueryHandler : IRequestHandler<GetTenantQuery, TenantDto?>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly UserManager<Domain.Entities.ApplicationUser> _userManager;

    public GetTenantQueryHandler(
        ITenantRepository tenantRepository,
        UserManager<Domain.Entities.ApplicationUser> userManager)
    {
        _tenantRepository = tenantRepository;
        _userManager = userManager;
    }

    public async Task<TenantDto?> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            return null;
        }

        var userCount = _userManager.Users.Count(u => u.TenantId == tenant.Id);

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            ContactEmail = tenant.ContactEmail,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            UserCount = userCount
        };
    }
}


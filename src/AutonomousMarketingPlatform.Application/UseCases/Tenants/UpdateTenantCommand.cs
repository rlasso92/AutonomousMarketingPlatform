using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Tenants;

/// <summary>
/// Comando para actualizar tenant.
/// </summary>
public class UpdateTenantCommand : IRequest<TenantDto>
{
    public Guid TenantId { get; set; }
    public UpdateTenantDto Tenant { get; set; } = null!;
}

/// <summary>
/// Handler para actualizar tenant.
/// </summary>
public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, TenantDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<UpdateTenantCommandHandler> _logger;

    public UpdateTenantCommandHandler(
        ITenantRepository tenantRepository,
        ILogger<UpdateTenantCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<TenantDto> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant {request.TenantId} no encontrado");
        }

        // Verificar si el subdomain ya está en uso por otro tenant
        if (!string.IsNullOrEmpty(request.Tenant.Subdomain) && 
            request.Tenant.Subdomain != tenant.Subdomain)
        {
            var existingBySubdomain = await _tenantRepository.GetBySubdomainAsync(
                request.Tenant.Subdomain, 
                cancellationToken);
            if (existingBySubdomain != null && existingBySubdomain.Id != request.TenantId)
            {
                throw new InvalidOperationException($"Ya existe un tenant con subdomain {request.Tenant.Subdomain}");
            }
        }

        // Actualizar campos
        tenant.Name = request.Tenant.Name;
        tenant.Subdomain = request.Tenant.Subdomain ?? string.Empty;
        tenant.ContactEmail = request.Tenant.ContactEmail ?? string.Empty;
        tenant.IsActive = request.Tenant.IsActive;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);

        _logger.LogInformation("Tenant actualizado: {TenantId}, Nombre: {Name}", 
            tenant.Id, tenant.Name);

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            ContactEmail = tenant.ContactEmail,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            UserCount = 0 // Se calculará en la query si es necesario
        };
    }
}


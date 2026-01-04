using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using AutonomousMarketingPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Infrastructure.Services;

/// <summary>
/// Implementación del repositorio de logs de aplicación.
/// </summary>
public class ApplicationLogRepository : IApplicationLogRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantRepository _tenantRepository;

    public ApplicationLogRepository(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        UserManager<ApplicationUser> userManager,
        ITenantRepository tenantRepository)
    {
        _dbContextFactory = dbContextFactory;
        _userManager = userManager;
        _tenantRepository = tenantRepository;
    }

    public async Task<List<ApplicationLogListDto>> GetLogsAsync(
        Guid? tenantId,
        Guid? userId,
        string? level,
        string? source,
        DateTime? fromDate,
        DateTime? toDate,
        int skip,
        int take,
        bool isSuperAdmin,
        CancellationToken cancellationToken = default)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var query = context.ApplicationLogs.AsQueryable();

        // Filtrar por tenant (si no es SuperAdmin o si se especifica un tenant)
        if (!isSuperAdmin && tenantId.HasValue)
        {
            query = query.Where(log => log.TenantId == tenantId);
        }
        else if (isSuperAdmin && tenantId.HasValue && tenantId != Guid.Empty)
        {
            query = query.Where(log => log.TenantId == tenantId);
        }

        // Filtrar por usuario
        if (userId.HasValue)
        {
            query = query.Where(log => log.UserId == userId);
        }

        // Filtrar por nivel
        if (!string.IsNullOrEmpty(level))
        {
            query = query.Where(log => log.Level == level);
        }

        // Filtrar por origen
        if (!string.IsNullOrEmpty(source))
        {
            query = query.Where(log => log.Source.Contains(source));
        }

        // Filtrar por fecha
        if (fromDate.HasValue)
        {
            query = query.Where(log => log.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(log => log.CreatedAt <= toDate.Value);
        }

        // Ordenar por fecha descendente y aplicar paginación
        var logs = await query
            .OrderByDescending(log => log.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        var result = new List<ApplicationLogListDto>();

        foreach (var log in logs)
        {
            string? tenantName = null;
            if (log.TenantId.HasValue)
            {
                var tenant = await _tenantRepository.GetByIdAsync(log.TenantId.Value, cancellationToken);
                tenantName = tenant?.Name;
            }

            string? userEmail = null;
            if (log.UserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(log.UserId.Value.ToString());
                userEmail = user?.Email;
            }

            result.Add(new ApplicationLogListDto
            {
                Id = log.Id,
                Level = log.Level,
                Message = log.Message.Length > 200 ? log.Message.Substring(0, 200) + "..." : log.Message,
                Source = log.Source,
                TenantId = log.TenantId,
                TenantName = tenantName,
                UserId = log.UserId,
                UserEmail = userEmail,
                Path = log.Path,
                HttpMethod = log.HttpMethod,
                CreatedAt = log.CreatedAt
            });
        }

        return result;
    }

    public async Task<ApplicationLogDto?> GetLogByIdAsync(
        Guid id,
        Guid? tenantId,
        bool isSuperAdmin,
        CancellationToken cancellationToken = default)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var log = await context.ApplicationLogs
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (log == null)
        {
            return null;
        }

        // Verificar permisos: si no es SuperAdmin, verificar que el log pertenezca al tenant
        if (!isSuperAdmin && tenantId.HasValue)
        {
            if (log.TenantId != tenantId)
            {
                return null; // No tiene permisos para ver este log
            }
        }

        string? tenantName = null;
        if (log.TenantId.HasValue)
        {
            var tenant = await _tenantRepository.GetByIdAsync(log.TenantId.Value, cancellationToken);
            tenantName = tenant?.Name;
        }

        string? userEmail = null;
        if (log.UserId.HasValue)
        {
            var user = await _userManager.FindByIdAsync(log.UserId.Value.ToString());
            userEmail = user?.Email;
        }

        return new ApplicationLogDto
        {
            Id = log.Id,
            Level = log.Level,
            Message = log.Message,
            Source = log.Source,
            TenantId = log.TenantId,
            TenantName = tenantName,
            UserId = log.UserId,
            UserEmail = userEmail,
            StackTrace = log.StackTrace,
            ExceptionType = log.ExceptionType,
            InnerException = log.InnerException,
            RequestId = log.RequestId,
            Path = log.Path,
            HttpMethod = log.HttpMethod,
            AdditionalData = log.AdditionalData,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            CreatedAt = log.CreatedAt,
            UpdatedAt = log.UpdatedAt,
            IsActive = log.IsActive
        };
    }
}


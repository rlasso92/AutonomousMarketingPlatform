using AutonomousMarketingPlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Infrastructure.Services;

/// <summary>
/// Servicio para inicializar roles del sistema.
/// </summary>
public class RoleSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<RoleSeeder> _logger;

    public RoleSeeder(RoleManager<ApplicationRole> roleManager, ILogger<RoleSeeder> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Crea los roles del sistema si no existen.
    /// </summary>
    public async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new { Name = "Owner", Description = "Dueño del tenant, acceso total" },
            new { Name = "Admin", Description = "Administrador del tenant" },
            new { Name = "Marketer", Description = "Marketer, puede crear/editar campañas" },
            new { Name = "Viewer", Description = "Solo lectura" }
        };

        foreach (var role in roles)
        {
            var exists = await _roleManager.RoleExistsAsync(role.Name);
            if (!exists)
            {
                var applicationRole = new ApplicationRole
                {
                    Name = role.Name,
                    NormalizedName = role.Name.ToUpper(),
                    Description = role.Description,
                    IsActive = true
                };

                var result = await _roleManager.CreateAsync(applicationRole);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Rol creado: {RoleName}", role.Name);
                }
                else
                {
                    _logger.LogError("Error al crear rol {RoleName}: {Errors}", 
                        role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}



using AutonomousMarketingPlatform.Application.UseCases.AI;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotFoundException = AutonomousMarketingPlatform.Application.UseCases.Campaigns.NotFoundException;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para gestión de MarketingPacks desde la interfaz web.
/// </summary>
[Authorize]
public class MarketingPacksController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<MarketingPacksController> _logger;

    public MarketingPacksController(
        IMediator mediator,
        ILogger<MarketingPacksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Elimina un MarketingPack (soft delete).
    /// </summary>
    [HttpPost]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = UserHelper.GetUserId(User);
            var tenantId = UserHelper.GetTenantId(User);

            if (!userId.HasValue || !tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var command = new DeleteMarketingPackCommand
            {
                TenantId = tenantId.Value,
                UserId = userId.Value,
                MarketingPackId = id
            };

            await _mediator.Send(command);

            TempData["SuccessMessage"] = "MarketingPack eliminado exitosamente.";
            return RedirectToAction("Index", "Dashboard");
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar MarketingPack {MarketingPackId}", id);
            TempData["ErrorMessage"] = "Error al eliminar el MarketingPack. Por favor, intente nuevamente.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}


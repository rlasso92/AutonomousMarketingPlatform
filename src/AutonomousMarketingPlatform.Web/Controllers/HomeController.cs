using AutonomousMarketingPlatform.Application.UseCases.Dashboard;
using AutonomousMarketingPlatform.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IMediator mediator, ILogger<HomeController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // TODO: Obtener TenantId del usuario autenticado
            var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var query = new GetDashboardDataQuery
            {
                TenantId = tenantId
            };

            var dashboardData = await _mediator.Send(query);
            return View(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar datos del dashboard");
            return View(new Application.DTOs.DashboardDto());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }
}


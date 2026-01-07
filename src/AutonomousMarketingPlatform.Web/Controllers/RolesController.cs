using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers;

[Authorize]
public class RolesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

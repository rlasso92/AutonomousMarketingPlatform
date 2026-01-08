using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers
{
    public class MarketingPacksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

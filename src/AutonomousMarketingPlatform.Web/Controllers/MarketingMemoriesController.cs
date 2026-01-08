using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers
{
    public class MarketingMemoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

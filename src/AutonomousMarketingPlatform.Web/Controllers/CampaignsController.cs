using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers
{
    public class CampaignsController : Controller
    {
        // This action serves the main page for the Campaigns module.
        // The page itself will then use JavaScript to call the API for data.
        public IActionResult Index()
        {
            return View();
        }
    }
}

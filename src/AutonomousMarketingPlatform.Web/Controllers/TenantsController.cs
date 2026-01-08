using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers
{
    public class TenantsController : Controller
    {
        // This action serves the main page for the Tenants module as a partial view.
        // The page itself will then use JavaScript to call the API for data.
        public IActionResult Index()
        {
            return PartialView();
        }
    }
}

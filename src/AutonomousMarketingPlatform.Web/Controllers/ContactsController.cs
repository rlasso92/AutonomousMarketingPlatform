using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers
{
    public class ContactsController : Controller
    {
        // This action serves the main page for the Contacts module.
        // The page itself will then use JavaScript to call the API for data.
        public IActionResult Index()
        {
            return View();
        }
    }
}

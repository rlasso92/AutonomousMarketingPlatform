using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

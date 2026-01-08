using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers
{
    public class TemplatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

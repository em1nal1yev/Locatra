using Microsoft.AspNetCore.Mvc;

namespace LocatraMain.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

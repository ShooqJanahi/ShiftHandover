using Microsoft.AspNetCore.Mvc;

namespace ShiftHandover.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult privacy()
        {
            return View();
        }
    }
}

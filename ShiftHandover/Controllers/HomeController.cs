using Microsoft.AspNetCore.Mvc;

namespace ShiftHandover.Controllers
{
    public class HomeController : Controller
    {
        // Action method that handles requests to the Privacy page
        public IActionResult privacy()
        {
            return View();
        }
    }
}

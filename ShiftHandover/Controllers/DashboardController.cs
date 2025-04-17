using Microsoft.AspNetCore.Mvc;

namespace ShiftHandover.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult UserDashboard()
        {
            return View();
        }
    }
}

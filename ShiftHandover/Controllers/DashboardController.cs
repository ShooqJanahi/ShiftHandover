using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ShiftHandover.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult UserDashboard()
        {
            // Check if the user is logged in
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                // User is not logged in, redirect to Login
                return RedirectToAction("Login", "Account");
            }

            if (role != "Supervisor")
            {
                // User is logged in but not Supervisor
                TempData["ErrorMessage"] = "You are not authorized to access the Supervisor Dashboard.";
                return RedirectToAction("Login", "Account"); // Or another Unauthorized page if you have
            }

            // If everything is OK
            return View();
        }
    }
}

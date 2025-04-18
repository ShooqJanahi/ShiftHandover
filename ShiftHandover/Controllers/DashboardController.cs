using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShiftHandover.Models;

namespace ShiftHandover.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        public IActionResult AdminDashboard()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Admin")
            {
                TempData["ErrorMessage"] = "You are not authorized to access the Admin Dashboard.";
                return RedirectToAction("Login", "Account");
            }

            // Example: you can load data you want to show on Admin Dashboard (optional)
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalShifts = _context.Shifts.Count();
            ViewBag.TotalAccidents = _context.ShiftLogs.Count(l => l.Type == "Accident");

            ViewBag.ClaimedShifts = _context.Shifts.Count(s => s.IsClaimed);
            ViewBag.UnclaimedShifts = _context.Shifts.Count(s => !s.IsClaimed);

            ViewBag.ShiftDates = _context.Shifts
                .OrderBy(s => s.StartTime)
                .Select(s => s.StartTime.ToString("yyyy-MM-dd"))
            .ToList();

            ViewBag.ShiftCounts = _context.Shifts
                .GroupBy(s => s.StartTime.Date)
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToList();

            return View();
        }



    }
}

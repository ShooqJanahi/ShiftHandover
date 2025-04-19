using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShiftHandover.Models;
using Humanizer;

namespace ShiftHandover.Controllers
{
    // The DashboardController handles rendering dashboards for both Supervisors and Admins.
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor - Dependency Injection of the database context
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard/UserDashboard
        public IActionResult UserDashboard()
        {
            // Check if the user is logged in
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                // No session exists, meaning user is not logged in
                return RedirectToAction("Login", "Account"); // Redirect to login page 
            }

            if (role != "Supervisor")
            {
                // user is logged in but does not have Supervisor role
                TempData["ErrorMessage"] = "You are not authorized to access the Supervisor Dashboard.";
                return RedirectToAction("Login", "Account"); // Redirect to login page 
            }

            // User is authenticated and authorized
            return View();
        }

        // GET: Dashboard/AdminDashboard
        public IActionResult AdminDashboard()
        {
            // Check if the user is logged in
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                // No session, redirect to login
                return RedirectToAction("Login", "Account");
            }

            if (role != "Admin")
            {
                //Logged in but not an Admin
                TempData["ErrorMessage"] = "You are not authorized to access the Admin Dashboard.";
                return RedirectToAction("Login", "Account");
            }

            // If the user is an Admin, load dashboard data
            // + Below, we are gathering data to show Admin some statistics:


            ViewBag.TotalUsers = _context.Users.Count(); // Total number of users registered
            ViewBag.TotalShifts = _context.Shifts.Count(); // Total number of shifts created
            ViewBag.TotalAccidents = _context.ShiftLogs.Count(l => l.Type == "Accident"); // Total number of accidents recorded in shift logs

            ViewBag.ClaimedShifts = _context.Shifts.Count(s => s.IsClaimed); // Number of shifts that have been claimed
            ViewBag.UnclaimedShifts = _context.Shifts.Count(s => !s.IsClaimed); // Number of shifts that are still unclaimed


            // + Preparing data for a chart 

            // List of dates when shifts start (to use as labels on a graph)
            ViewBag.ShiftDates = _context.Shifts
                .OrderBy(s => s.StartTime)
                .Select(s => s.StartTime.ToString("yyyy-MM-dd"))
            .ToList();

            // List of shift counts grouped by each start date (number of shifts per day)
            ViewBag.ShiftCounts = _context.Shifts
                .GroupBy(s => s.StartTime.Date)
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToList();
            // Now that all the Admin data is prepared, render the AdminDashboard view
            return View();
        }



    }
}

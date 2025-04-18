using Microsoft.AspNetCore.Mvc;
using ShiftHandover.Models;
using ShiftHandover.Helpers;
using System.Linq;

namespace ShiftHandover.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }





        // GET: Admin/AddUser
        public IActionResult AddUser()
        {
            return View();
        }

        // POST: Admin/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check uniqueness
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(model);
            }
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }
            if (_context.Users.Any(u => u.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Phone number already exists.");
                return View(model);
            }

            // Hash password before saving
            model.PasswordHash = PasswordHelper.Hash(model.PasswordHash);

            // Mark user active by default
            model.IsActive = true;

            _context.Users.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction("AddUser");
        }

        public IActionResult ListUsers(string searchTerm, string statusFilter)
        {
            var users = _context.Users.ToList();

            // 🔥 Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u =>
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm) ||
                    u.Username.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.PhoneNumber.ToLower().Contains(searchTerm) ||
                    u.Department.ToLower().Contains(searchTerm) ||
                    u.RoleTitle.ToLower().Contains(searchTerm) ||
                    u.UserId.ToString().Contains(searchTerm) // 🎯 Allow search by User ID
                ).ToList();
            }

            // 🔥 Apply status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Active")
                    users = users.Where(u => u.IsActive).ToList();
                else if (statusFilter == "Inactive")
                    users = users.Where(u => !u.IsActive).ToList();
            }

            return View(users);
        }

        public IActionResult ViewUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            var userShifts = _context.Shifts
                            .Where(s => s.SupervisorId == user.UserId.ToString())
                            .OrderByDescending(s => s.StartTime)
                            .ToList();

            ViewBag.UserShifts = userShifts;

            return View(user);
        }



    }
}

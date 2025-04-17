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
    }
}

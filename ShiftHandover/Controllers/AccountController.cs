using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShiftHandover.Models;
using ShiftHandover.Helpers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.IsActive);
        if (user != null && PasswordHelper.Hash(model.Password) == user.PasswordHash)
        {
            // Save Username and Role in session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.RoleTitle); // Save the user's role too

            if (user.RoleTitle == "Supervisor")
            {
                // Supervisor is allowed
                return RedirectToAction("UserDashboard", "Dashboard");
            }
            else
            {
                // Not a Supervisor ➔ Show warning
                TempData["Warning"] = "You do not have the privilege to access the system.";
                HttpContext.Session.Clear(); // Clear session for safety
                return RedirectToAction("Login");
            }
        }

        ViewBag.Message = "Invalid credentials.";
        return View(model);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}

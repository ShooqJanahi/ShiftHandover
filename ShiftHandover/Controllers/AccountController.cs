using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShiftHandover.Models;
using ShiftHandover.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


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
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.IsActive);

        if (user != null && PasswordHelper.Hash(model.Password) == user.PasswordHash)
        {
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.RoleTitle);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.RoleTitle)
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // 🔥 REDIRECTION BASED ON ROLE
            if (user.RoleTitle == "Supervisor")
            {
                return RedirectToAction("UserDashboard", "Dashboard");
            }
            else if (user.RoleTitle == "Admin")
            {
                return RedirectToAction("AdminDashboard", "Dashboard");
            }
            else
            {
                TempData["Warning"] = "You do not have the privilege to access the system.";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }
        }

        ViewBag.Message = "Invalid credentials.";
        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        // 🔥 Clear the authentication cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 🔥 Clear session
        HttpContext.Session.Clear();

        return RedirectToAction("Login");
    }



    //backend endpoint for session check
    [HttpGet]
    public IActionResult CheckSession()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return Unauthorized(); // 🛑 No session

        return Ok(); // ✅ Session is alive
    }
}


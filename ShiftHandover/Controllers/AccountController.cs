using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShiftHandover.Models;
using ShiftHandover.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

// Controller responsible for user authentication-related operations
public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    // Dependency Injection of the database context
    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Displays the Login view (GET request)
    public IActionResult Login()
    {
        return View();
    }

    // Handles Login form submission
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Check if the model state is valid (all required fields are correctly filled)
        if (!ModelState.IsValid)
            return View(model);

        // Fetch the user from the database by username
        var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

        if (user == null || PasswordHelper.Hash(model.Password) != user.PasswordHash)
        {
            ViewBag.Message = "Invalid credentials.";
            return View(model);
        }

        // Check if the user account is active
        if (!user.IsActive)
        {
            ViewBag.Message = "Your account is inactive. Please contact the administrator.";
            return View(model);
        }


        // If user exists and credentials are valid
        if (user != null && PasswordHelper.Hash(model.Password) == user.PasswordHash)
        {
            // Store basic user information in session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.RoleTitle);
            HttpContext.Session.SetString("DepartmentId", user.DepartmentId.ToString());


            // Create a list of claims (Name and Role) for authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.RoleTitle)
            };

            // Create a ClaimsIdentity with the claims list and specify the authentication scheme
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Define authentication properties (session persistence and expiry time)
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            // Sign in the user using cookie authentication
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // Redirect user based on their role
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
                // If user has no suitable role, deny access
                TempData["Warning"] = "You do not have the privilege to access the system.";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }
        }
        // If login fails (this block is unlikely to be hit after successful login)
        ViewBag.Message = "Invalid credentials.";
        return View(model);
    }

    // Handles user logout
    public async Task<IActionResult> Logout()
    {
        //  Clear the authentication cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        //  Clear all session data
        HttpContext.Session.Clear();

        // Redirect to login page after logout
        return RedirectToAction("Login");
    }

    // Backend endpoint for checking if the session is still active 
    [HttpGet]
    public IActionResult CheckSession()
    {
        var username = HttpContext.Session.GetString("Username");

        // If username is missing from session, session is considered expired or invalid
        if (string.IsNullOrEmpty(username))
            return Unauthorized(); // No session

        return Ok(); // Session is active
    }



}


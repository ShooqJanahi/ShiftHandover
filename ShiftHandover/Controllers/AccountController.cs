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
            HttpContext.Session.SetString("Username", user.Username);
            return RedirectToAction("Index", "Dashboard");
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

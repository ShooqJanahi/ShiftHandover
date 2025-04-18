using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ShiftHandover.Models;
using ShiftHandover.Helpers;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace ShiftHandover.Controllers
{
    
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }


        private bool IsAdmin()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                // Not logged in
                return false;
            }

            if (role != "Admin")
            {
                // Logged in but wrong role
                TempData["ErrorMessage"] = "You do not have the required privileges.";
                return false;
            }

            return true;
        }



        // GET: Admin/AddUser
        public IActionResult AddUser()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Departments = _context.Departments.ToList(); // ✅ fetch departments
            return View();
        }



        // POST: Admin/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(User model)
        {
            ViewBag.Departments = _context.Departments.ToList(); // ✅ Always load departments before validation

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 🔥 Check uniqueness
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

            // 🔥 Store the plain password temporarily
            string plainPassword = model.PasswordHash;

            // ✅ Password Validation
            var password = model.PasswordHash;
            var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, passwordPattern))
            {
                ModelState.AddModelError("PasswordHash", "Password must be at least 8 characters, include uppercase, lowercase, number, and special character.");
                return View(model);
            }

            // ✅ Hash password before saving
            model.PasswordHash = PasswordHelper.Hash(model.PasswordHash);

            model.IsActive = true;

            _context.Users.Add(model);
            _context.SaveChanges();

            SendEmailHelper.Send(model.Email, model.Username, plainPassword);

            TempData["SuccessMessage"] = "User created successfully and credentials sent!";
            return RedirectToAction("AddUser");
        }

        public IActionResult ListUsers(string searchTerm, string statusFilter)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var users = _context.Users
            .Include(u => u.Department) 
            .ToList();


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
                   (u.Department != null && u.Department.DepartmentName.ToLower().Contains(searchTerm)) || // DepartmentName search
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

        [HttpPost]
        public IActionResult GenerateUserReport(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            var shifts = _context.Shifts
                .Where(s => s.SupervisorId == user.UserId.ToString())
                .OrderBy(s => s.StartTime)
                .ToList();

            var shiftIds = shifts.Select(s => s.Id).ToList();
            var shiftLogs = _context.ShiftLogs
                .Where(log => shiftIds.Contains(log.ShiftId))
                .OrderByDescending(log => log.LogTime)
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Text("ShiftHandover - User Report")
                                 .FontSize(24)
                                 .SemiBold()
                                 .FontColor("#007ACC")
                                 .AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        // User Info
                        col.Item().Border(1).BorderColor("#007ACC").Padding(10).Column(innerCol =>
                        {
                            innerCol.Item().Text($"User ID: {user.UserId}");
                            innerCol.Item().Text($"Name: {user.FirstName} {user.LastName}");
                            innerCol.Item().Text($"Username: {user.Username}");
                            innerCol.Item().Text($"Email: {user.Email}");
                            innerCol.Item().Text($"Phone: {user.PhoneNumber}");
                            innerCol.Item().Text($"Department: {user.Department}");
                            innerCol.Item().Text($"Role: {user.RoleTitle}");
                            innerCol.Item().Text($"Status: {(user.IsActive ? "Active" : "Inactive")}");
                        });

                        // Shifts Section
                        col.Item().PaddingTop(20).Text("Shifts History").FontSize(18).Bold().FontColor("#007ACC");

                        if (shifts.Any())
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Shift ID").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Shift Type").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Location").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Start Time").SemiBold();
                                    header.Cell().Element(CellStyle).Text("End Time").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Status").SemiBold();
                                });

                                foreach (var shift in shifts)
                                {
                                    table.Cell().Element(CellStyle).Text($"{shift.Id}");
                                    table.Cell().Element(CellStyle).Text(shift.ShiftType ?? "-");
                                    table.Cell().Element(CellStyle).Text(shift.Location ?? "-");
                                    table.Cell().Element(CellStyle).Text(shift.StartTime.ToString("g"));
                                    table.Cell().Element(CellStyle).Text(shift.EndTime.HasValue ? shift.EndTime.Value.ToString("g") : "N/A");
                                    table.Cell().Element(CellStyle).Text(
                                        shift.IsClosed ? "Closed" : shift.IsClaimed ? "Claimed" : "Unclaimed"
                                    );
                                }
                            });
                        }
                        else
                        {
                            col.Item().Text("No shifts assigned to this user.").Italic();
                        }

                        // Logs Section
                        col.Item().PaddingTop(20).Text("Shift Logs").FontSize(18).Bold().FontColor("#007ACC");

                        if (shiftLogs.Any())
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(6);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Time").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Type").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Severity").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Shift ID").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Description").SemiBold();
                                });

                                foreach (var log in shiftLogs)
                                {
                                    table.Cell().Element(CellStyle).Text($"{log.LogTime:g}");
                                    table.Cell().Element(CellStyle).Text(log.Type ?? "-");
                                    table.Cell().Element(CellStyle).Text(log.Severity ?? "-");
                                    table.Cell().Element(CellStyle).Text(log.ShiftId.ToString());
                                    table.Cell().Element(CellStyle).Text(log.Description ?? "-");
                                }
                            });
                        }
                        else
                        {
                            col.Item().Text("No logs recorded.").Italic();
                        }

                    });

                    page.Footer().AlignCenter().Text($"Generated on {DateTime.Now:f}").FontSize(10).FontColor(Colors.Grey.Darken2);
                });
            });

            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"UserReport_{user.Username}.pdf");

            // Helper
            IContainer CellStyle(IContainer container) => container.PaddingVertical(5).PaddingHorizontal(3);
        }

        [HttpPost]
        public IActionResult DeactivateUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            user.IsActive = false;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "User deactivated successfully.";
            return RedirectToAction("ViewUser", new { id = id });
        }

        [HttpPost]
        public IActionResult ActivateUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            user.IsActive = true;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "User activated successfully.";
            return RedirectToAction("ViewUser", new { id = id });
        }


    }



}


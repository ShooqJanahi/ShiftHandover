using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ShiftHandover.Models;
using ShiftHandover.Helpers;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShiftHandover.Controllers
{
    // Controller managing admin-related functionalities
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor - inject the application's DbContext
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Private helper method to check if the current user is an Admin
        private bool IsAdmin()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                // User is not logged in
                return false;
            }

            if (role != "Admin")
            {
                // User is logged in but doesn't have Admin role
                TempData["ErrorMessage"] = "You do not have the required privileges.";
                return false;
            }

            return true;
        }


        // GET: Admin/AddUser
        //Display the Add User form
        public IActionResult AddUser()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Departments = _context.Departments.ToList(); // Load departments list for dropdown
            return View();
        }


        // POST: Admin/AddUser
        // Handle form submission for creating a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(User model)
        {
            ViewBag.Departments = _context.Departments.ToList(); // Always load departments in case form reloads due to error

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate uniqueness of username, email, and phone number
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

            // Temporarily store plain password for emailing
            string plainPassword = model.PasswordHash;

            // Validate password format (uppercase, lowercase, number, special character, min 8 chars)
           
            var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(model.PasswordHash, passwordPattern))
            {
                ModelState.AddModelError("PasswordHash", "Password must be at least 8 characters, include uppercase, lowercase, number, and special character.");
                return View(model);
            }

            // Hash password before saving into the database
            model.PasswordHash = PasswordHelper.Hash(model.PasswordHash);

            model.IsActive = true; // Set new users as active by default

            // Save the user
            _context.Users.Add(model);
            _context.SaveChanges();

            // Send credentials to the user via email
            SendEmailHelper.Send(model.Email, model.Username, plainPassword);

            TempData["SuccessMessage"] = "User created successfully and credentials sent!";
            return RedirectToAction("AddUser");
        }

        //Display all users, optionally filtered by search term or status
        // GET: Admin/ListUsers
        public IActionResult ListUsers(string searchTerm, string statusFilter)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch users with their departments
            var users = _context.Users
            .Include(u => u.Department) 
            .ToList();


            // Apply search functionality
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

            // Apply status (Active/Inactive) filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Active")
                    users = users.Where(u => u.IsActive).ToList();
                else if (statusFilter == "Inactive")
                    users = users.Where(u => !u.IsActive).ToList();
            }

            return View(users);
        }

        //View user details including their shifts
        // GET: Admin/ViewUser
        public IActionResult ViewUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Fetch shifts assigned to the user
            var userShifts = _context.Shifts
                            .Where(s => s.SupervisorId == user.UserId.ToString())
                            .OrderByDescending(s => s.StartTime)
                            .ToList();

            ViewBag.UserShifts = userShifts;

            return View(user);
        }

        // POST: Admin/GenerateUserReport
        //Generate a detailed PDF report for a user
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

            // Building the PDF document using QuestPDF
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

                        // User Information
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

                        // Shifts History Section
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

                        // Shift Logs Section
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

            // Generate PDF and return as file download
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"UserReport_{user.Username}.pdf");

            // Helper for table cell styling
            IContainer CellStyle(IContainer container) => container.PaddingVertical(5).PaddingHorizontal(3);
        }

        // POST: Admin/DeactivateUser
        //Set user as inactive
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

        // POST: Admin/ActivateUser
        //Set user as active
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


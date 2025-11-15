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
            // Always reload departments for the dropdown
            ViewBag.Departments = _context.Departments.ToList();

            // Ignore validation for navigation property Department
            ModelState.Remove("Department");

            // 1) Data-annotation validation (required fields, phone regex, etc.)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2) Custom uniqueness checks
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

            // 3) Custom password pattern check
            var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.PasswordHash, passwordPattern))
            {
                ModelState.AddModelError("PasswordHash", "Password must be at least 8 characters, include uppercase, lowercase, number, and special character.");
                return View(model);
            }

            // Temporarily keep plain password for email
            string plainPassword = model.PasswordHash;

            // 4) Hash & save
            model.PasswordHash = PasswordHelper.Hash(model.PasswordHash);
            model.IsActive = true;

            _context.Users.Add(model);
            _context.SaveChanges();

            // 5) Send email & redirect
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


            // search functionality
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

            // status (Active/Inactive) filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Active")
                    users = users.Where(u => u.IsActive).ToList();
                else if (statusFilter == "Inactive")
                    users = users.Where(u => !u.IsActive).ToList();
            }

            return View(users);
        }

        // View user details including their shifts
        // GET: Admin/ViewUser
        public IActionResult ViewUser(int id)
        {
            var user = _context.Users
                .Include(u => u.Department)          // include Department
                .FirstOrDefault(u => u.UserId == id);

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
        // Generate a detailed PDF report for a user
        [HttpPost]
        public IActionResult GenerateUserReport(int id)
        {
            var user = _context.Users
                .Include(u => u.Department)
                .FirstOrDefault(u => u.UserId == id);

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

            // ---- summary numbers ----
            var totalShifts = shifts.Count;
            var activeShifts = shifts.Count(s => s.IsClaimed && !s.IsClosed);
            var closedShifts = shifts.Count(s => s.IsClosed);
            var upcomingShifts = shifts.Count(s => !s.IsClosed && s.StartTime >= DateTime.Now);
            var totalLogs = shiftLogs.Count;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.Grey.Lighten4);

                    // ---------- HEADER ----------
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("ShiftHandover")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);

                            col.Item().Text("User Activity Report")
                                .FontSize(22)
                                .SemiBold()
                                .FontColor("#007ACC");
                        });

                        row.ConstantItem(180).Column(col =>
                        {
                            col.Item().AlignRight().Text($"User: {user.Username}")
                                .FontSize(10)
                                .SemiBold();

                            col.Item().AlignRight().Text($"Generated: {DateTime.Now:f}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken2);
                        });
                    });

                    // ---------- CONTENT ----------
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(15);

                        // ---- User details card ----
                        col.Item().Background(Colors.White)
                            .Border(1).BorderColor("#007ACC")
                            .Padding(12)
                            .Column(section =>
                            {
                                section.Spacing(4);

                                section.Item().Text("User Details")
                                    .FontSize(14)
                                    .SemiBold()
                                    .FontColor("#007ACC");

                                section.Item()
                                    .BorderBottom(1)
                                    .BorderColor("#007ACC")
                                    .PaddingBottom(5);

                                section.Item().Row(r =>
                                {
                                    r.RelativeItem().Column(c =>
                                    {
                                        LabelValue(c, "User ID", user.UserId.ToString());
                                        LabelValue(c, "Name", $"{user.FirstName} {user.LastName}");
                                        LabelValue(c, "Username", user.Username);
                                        LabelValue(c, "Email", user.Email);
                                    });

                                    r.RelativeItem().Column(c =>
                                    {
                                        LabelValue(c, "Phone", user.PhoneNumber);
                                        LabelValue(c, "Department", user.Department?.DepartmentName ?? "N/A");
                                        LabelValue(c, "Role", user.RoleTitle);
                                        LabelValue(c, "Status", user.IsActive ? "Active" : "Inactive");
                                    });
                                });
                            });

                        // ---- Summary cards ----
                        col.Item().Row(row =>
                        {
                            row.Spacing(8);

                            SummaryCard(row, "Total Shifts", totalShifts.ToString());
                            SummaryCard(row, "Active Shifts", activeShifts.ToString());
                            SummaryCard(row, "Closed Shifts", closedShifts.ToString());
                            SummaryCard(row, "Upcoming Shifts", upcomingShifts.ToString());
                            SummaryCard(row, "Total Logs", totalLogs.ToString());
                        });

                        // ---- Shifts history table ----
                        col.Item().PaddingTop(10).Column(section =>
                        {
                            section.Item().Text("Shifts History")
                                .FontSize(14)
                                .SemiBold()
                                .FontColor("#007ACC");

                            section.Item()
                                .BorderBottom(1)
                                .BorderColor("#007ACC")
                                .PaddingBottom(5);

                            if (shifts.Any())
                            {
                                section.Item().Background(Colors.White)
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(5)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1.2f);   // Shift ID
                                            columns.RelativeColumn(1.8f);   // Type
                                            columns.RelativeColumn(2f);     // Location
                                            columns.RelativeColumn(2f);     // Start
                                            columns.RelativeColumn(2f);     // End
                                            columns.RelativeColumn(1.5f);   // Status
                                        });

                                        // Header
                                        table.Header(header =>
                                        {
                                            header.Cell().Element(HeaderCell).Text("Shift ID").SemiBold();
                                            header.Cell().Element(HeaderCell).Text("Shift Type").SemiBold();
                                            header.Cell().Element(HeaderCell).Text("Location").SemiBold();
                                            header.Cell().Element(HeaderCell).Text("Start Time").SemiBold();
                                            header.Cell().Element(HeaderCell).Text("End Time").SemiBold();
                                            header.Cell().Element(HeaderCell).Text("Status").SemiBold();
                                        });

                                        var indexedShifts = shifts.Select((s, idx) => new { s, idx });

                                        foreach (var item in indexedShifts)
                                        {
                                            bool zebra = item.idx % 2 == 1;

                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text(item.s.Id.ToString());
                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text(item.s.ShiftType ?? "-");
                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text(item.s.Location ?? "-");
                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text(item.s.StartTime.ToString("g"));
                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text(item.s.EndTime.HasValue
                                                    ? item.s.EndTime.Value.ToString("g")
                                                    : "N/A");
                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text(item.s.IsClosed
                                                    ? "Closed"
                                                    : item.s.IsClaimed ? "Claimed" : "Unclaimed");
                                        }
                                    });
                            }
                            else
                            {
                                section.Item().Text("No shifts assigned to this user.")
                                    .Italic()
                                    .FontColor(Colors.Grey.Darken1);
                            }
                        });

                        // ---- Shift logs table ----
                        col.Item().PaddingTop(10).Column(section =>
                        {
                            section.Item().Text("Shift Logs")
                                .FontSize(14)
                                .SemiBold()
                                .FontColor("#007ACC");

                            section.Item()
                                .BorderBottom(1)
                                .BorderColor("#007ACC")
                                .PaddingBottom(5);

                            if (shiftLogs.Any())
                            {
                                section.Item().Background(Colors.White)
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(5)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);   // Time
                                            columns.RelativeColumn(1.5f);// Type
                                            columns.RelativeColumn(1.5f);// Severity
                                            columns.RelativeColumn(1.5f);// Shift ID
                                            columns.RelativeColumn(5);   // Description
                                        });

                                        // Header
                                        table.Header(header =>
                                        {
                                            header.Cell().Element(HeaderCell).Text("Time").SemiBold();
                                            header.Cell().Element(HeaderCell).Text("Type").SemiBold();
                                            header.Cell().Element(HeaderCell).Text("Severity").SemiBold();
                                            header.Cell().Element(HeaderCell).Text("Shift ID").SemiBold();
                                            header.Cell().Element(HeaderCell).Text("Description").SemiBold();
                                        });

                                        var indexedLogs = shiftLogs.Select((log, idx) => new { log, idx });

                                        foreach (var item in indexedLogs)
                                        {
                                            bool zebra = item.idx % 2 == 1;

                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text($"{item.log.LogTime:g}");
                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text(item.log.Type ?? "-");
                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text(item.log.Severity ?? "-");
                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text(item.log.ShiftId.ToString());
                                            table.Cell().Element(c => DataCell(c, zebra))
                                                .Text(string.IsNullOrWhiteSpace(item.log.Description)
                                                    ? "-"
                                                    : item.log.Description);
                                        }
                                    });
                            }
                            else
                            {
                                section.Item().Text("No logs recorded for this user.")
                                    .Italic()
                                    .FontColor(Colors.Grey.Darken1);
                            }
                        });
                    });

                    // ---------- FOOTER ----------
                    page.Footer().AlignCenter()
                        .Text("ShiftHandover – Confidential · For internal use only")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken2);
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"UserReport_{user.Username}.pdf");

            // ---------- local helper functions ----------

            void LabelValue(ColumnDescriptor col, string label, string? value)
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(90).Text(label + ":")
                        .SemiBold()
                        .FontSize(10);

                    row.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value)
                        .FontSize(10);
                });
            }

            void SummaryCard(RowDescriptor row, string title, string value)
            {
                row.RelativeItem().Background(Colors.White)
                    .Border(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(8)
                    .Column(col =>
                    {
                        col.Item().Text(title)
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text(value)
                            .FontSize(12)
                            .SemiBold()
                            .FontColor("#007ACC");
                    });
            }

            IContainer HeaderCell(IContainer container) =>
                container.Background("#007ACC")
                    .PaddingVertical(4).PaddingHorizontal(3)
                    .DefaultTextStyle(t => t.FontColor(Colors.White).FontSize(10));

            IContainer DataCell(IContainer container, bool zebra) =>
                container.PaddingVertical(4).PaddingHorizontal(3)
                    .Background(zebra ? Colors.Grey.Lighten4 : Colors.White)
                    .DefaultTextStyle(t => t.FontSize(9));
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


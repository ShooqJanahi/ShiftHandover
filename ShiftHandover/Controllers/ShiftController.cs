using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShiftHandover.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class ShiftController : Controller
{
    private readonly ApplicationDbContext _context;

    public ShiftController(ApplicationDbContext context)
    {
        _context = context;
    }

    // This action returns all claimed shifts as JSON
    [HttpGet]
    public IActionResult GetShifts()
    {
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(); // 🚨 Make sure user is logged in
        }

        var shifts = _context.Shifts
            .Where(s => s.SupervisorName == username && s.IsClaimed) // ✅ Only the shifts claimed by this user
            .Select(s => new
            {
                id = s.Id,
                title = s.ShiftType + " Shift - " + s.Location,
                start = s.StartTime.ToString("s"), // ISO format
                end = s.EndTime.HasValue ? s.EndTime.Value.ToString("s") : null,
                color = s.IsClosed ? "#6c757d" : "#28a745" // Grey if closed, Green if open
            })
            .ToList();

        return Json(shifts);
    }

    [HttpGet]
    public IActionResult ViewShift(int id)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);
        if (shift == null)
            return NotFound();



        ViewBag.ShiftLogs = _context.ShiftLogs
            .Where(l => l.ShiftId == id)
            .OrderByDescending(l => l.LogTime)
            .ToList();

        ViewBag.Usernames = _context.Users.Select(u => u.Username).ToList();

        return View(shift);
    }



    [HttpPost]
    public IActionResult CloseShift(int id)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToAction("Login", "Account");



        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);

        if (shift == null)
        {
            return NotFound();
        }

        if (shift.SupervisorName != username || !shift.IsClaimed || shift.IsClosed)
        {
            return Unauthorized(); // 🔥 Protect ownership
        }

        shift.IsClosed = true;
        _context.SaveChanges();

        return RedirectToAction("ViewShift", new { id = id });
    }


    [HttpGet]
    public IActionResult ListAvailableShifts(string searchTerm, string shiftTypeFilter, string statusFilter)
    {
        var role = HttpContext.Session.GetString("Role");

        List<Shift> shifts;

        // 🔥 Check the role: Admin sees all shifts, Supervisor sees only not closed
        if (role == "Admin")
        {
            shifts = _context.Shifts.ToList(); // Admin sees ALL shifts
        }
        else
        {
            shifts = _context.Shifts.Where(s => !s.IsClosed).ToList(); // Supervisor sees only open shifts
        }

        // 🔥 Apply search
        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();

            shifts = shifts.Where(s =>
                (s.ShiftType != null && s.ShiftType.ToLower().Contains(searchTerm)) ||
                (s.Location != null && s.Location.ToLower().Contains(searchTerm)) ||
                (s.SupervisorName != null && s.SupervisorName.ToLower().Contains(searchTerm)) ||
                (s.StartTime.ToString("dddd").ToLower().Contains(searchTerm)) ||
                (s.StartTime.ToString("f").ToLower().Contains(searchTerm)) ||
                (s.EndTime.HasValue && s.EndTime.Value.ToString("f").ToLower().Contains(searchTerm)) ||
                (s.IsClaimed ? "claimed".Contains(searchTerm) : "unclaimed".Contains(searchTerm))
            ).ToList();
        }

        // 🔥 Apply additional filters
        if (!string.IsNullOrEmpty(shiftTypeFilter))
        {
            shifts = shifts.Where(s => s.ShiftType != null && s.ShiftType.Equals(shiftTypeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "Claimed")
                shifts = shifts.Where(s => s.IsClaimed).ToList();
            else if (statusFilter == "Unclaimed")
                shifts = shifts.Where(s => !s.IsClaimed).ToList();
        }

        return View(shifts);
    }


    [HttpPost]
    public IActionResult Claim(int id)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);

        if (shift != null && !shift.IsClaimed)
        {
            var username = HttpContext.Session.GetString("Username");

            if (username != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == username);

                if (user != null)
                {
                    shift.IsClaimed = true;
                    shift.SupervisorId = user.UserId.ToString(); // Save the UserId
                    shift.SupervisorName = user.Username; // Save the Username ONLY

                    _context.SaveChanges();
                }
            }
        }

        return RedirectToAction("UserDashboard", "Dashboard");
    }


    [HttpPost]
    public IActionResult GenerateReport(int id)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);
        if (shift == null)
            return NotFound();

        var shiftLogs = _context.ShiftLogs
             .Where(log => log.ShiftId == id)
             .OrderByDescending(log => log.LogTime) // 👈 Add this
             .ToList();


        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("ShiftHandover - Shift Report")
                             .FontSize(24)
                             .SemiBold()
                             .FontColor("#A30020")
                             .AlignCenter();

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Border(1).BorderColor("#A30020").Padding(10).Column(innerCol =>
                    {
                        innerCol.Item().Text($"Shift ID: {shift.Id}");
                      
                        innerCol.Item().Text($"Supervisor: {shift.SupervisorName}");
                        innerCol.Item().Text($"Location: {shift.Location}");
                        innerCol.Item().Text($"Shift Type: {shift.ShiftType}");
                        innerCol.Item().Text($"Start Time: {shift.StartTime:f}");
                        innerCol.Item().Text($"End Time: {(shift.EndTime.HasValue ? shift.EndTime.Value.ToString("f") : "N/A")}");
                        innerCol.Item().Text($"Total Manpower: {shift.TotalManpower}");
                        innerCol.Item().Text($"Notes: {shift.Notes}");
                        innerCol.Item().Text($"Shift Status: {(shift.IsClosed ? "Closed" : shift.IsClaimed ? "Claimed" : "Available")}");
                    });

                    col.Item().PaddingTop(20).Text("Shift Logs:").FontSize(18).Bold().FontColor("#A30020");

                    if (shiftLogs.Any())
                    {
                        // Make a nice table of logs
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Time
                                columns.RelativeColumn(2); // Type
                                columns.RelativeColumn(2); // Severity
                                columns.RelativeColumn(3); // Involved Person
                                columns.RelativeColumn(2); // Manpower Count
                                columns.RelativeColumn(5); // Description
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Time").SemiBold();
                                header.Cell().Element(CellStyle).Text("Type").SemiBold();
                                header.Cell().Element(CellStyle).Text("Severity").SemiBold();
                                header.Cell().Element(CellStyle).Text("Involved Person").SemiBold();
                                header.Cell().Element(CellStyle).Text("Manpower Count").SemiBold();
                                header.Cell().Element(CellStyle).Text("Description").SemiBold();
                            });

                            // Rows
                            foreach (var log in shiftLogs)
                            {
                                table.Cell().Element(CellStyle).Text($"{log.LogTime:g}");
                                table.Cell().Element(CellStyle).Text(log.Type);
                                table.Cell().Element(CellStyle).Text(log.Severity);
                                table.Cell().Element(CellStyle).Text(string.IsNullOrEmpty(log.InvolvedPerson) ? "-" : log.InvolvedPerson);
                                table.Cell().Element(CellStyle).Text(log.ManpowerCount.HasValue ? log.ManpowerCount.ToString() : "-");
                                table.Cell().Element(CellStyle).Text(log.Description);
                            }
                        });
                    }
                    else
                    {
                        col.Item().Text("No logs recorded for this shift.").Italic().FontColor(Colors.Grey.Darken1);
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text($"Generated on {DateTime.Now:f}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken2);
            });
        });

        byte[] pdfBytes = document.GeneratePdf();

        return File(pdfBytes, "application/pdf", $"ShiftReport_{shift.Id}.pdf");

        // Helper method
        IContainer CellStyle(IContainer container)
        {
            return container.PaddingVertical(5).PaddingHorizontal(2);
        }
    }

    [HttpGet]
    public IActionResult ShiftHistory(string searchTerm)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToAction("Login", "Account");

        // 🔥 Step 1: Fetch the shifts first from the database
        var shifts = _context.Shifts
            .Where(s => s.SupervisorName == username && s.IsClosed)
            .OrderByDescending(s => s.StartTime)
            .ToList(); // ⚡ Load into memory first

        // 🔥 Step 2: Do the search in memory
        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();

            shifts = shifts.Where(s =>
                s.Id.ToString().Contains(searchTerm) ||
                (s.Location != null && s.Location.ToLower().Contains(searchTerm)) ||
                (s.ShiftType != null && s.ShiftType.ToLower().Contains(searchTerm)) ||
                s.StartTime.ToString("f").ToLower().Contains(searchTerm) ||
                (s.EndTime.HasValue && s.EndTime.Value.ToString("f").ToLower().Contains(searchTerm)) ||
                (s.TotalManpower.HasValue && s.TotalManpower.Value.ToString().Contains(searchTerm)) ||
                (s.Notes != null && s.Notes.ToLower().Contains(searchTerm))
            ).ToList();
        }

        return View(shifts);
    }


    [HttpPost]
    public IActionResult LogShift(int id, string Type, string Description, DateTime LogTime, string InvolvedPerson, string Severity)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);
        if (shift == null)
            return NotFound();

        var username = HttpContext.Session.GetString("Username");

        // 🚨 Check if shift is claimed and not closed
        if (!shift.IsClaimed || shift.IsClosed || shift.SupervisorName != username)
        {
            TempData["LogError"] = "You are not allowed to log activities for this shift.";
            return RedirectToAction("ViewShift", new { id = id });
        }

        // 🚨 Check if LogTime is inside the shift timing
        // 🚨 Check if LogTime is inside the shift timing (with tolerance)
        if (LogTime < shift.StartTime.AddSeconds(-1) || (shift.EndTime.HasValue && LogTime > shift.EndTime.Value.AddSeconds(1)))
        {
            TempData["LogError"] = "Log Time must be within your shift's Start and End times.";
            return RedirectToAction("ViewShift", new { id = id });
        }


        // 🚨 Only allow Accident, Incident, Manpower
        var allowedTypes = new[] { "Accident", "Incident", "Manpower" };
        if (!allowedTypes.Contains(Type))
        {
            TempData["LogError"] = "Invalid log type selected.";
            return RedirectToAction("ViewShift", new { id = id });
        }

        int? manpowerCount = null;
        string involvedPersonValue = InvolvedPerson;

        if (Type == "Manpower")
        {
            if (!int.TryParse(InvolvedPerson, out int manpower))
            {
                TempData["LogError"] = "Please enter a valid manpower number.";
                return RedirectToAction("ViewShift", new { id = id });
            }

            manpowerCount = manpower;
            involvedPersonValue = null;

            // ✅ Update Shift TotalManpower
            shift.TotalManpower = (shift.TotalManpower ?? 0) + manpower;
        }
        else
        {
            var userExists = _context.Users.Any(u => u.Username == InvolvedPerson);
            if (!userExists)
            {
                TempData["LogError"] = "Involved Person username does not exist.";
                return RedirectToAction("ViewShift", new { id = id });
            }
        }

        var newLog = new ShiftLog
        {
            ShiftId = id,
            Type = Type,
            Description = Description,
            LogTime = LogTime,
            Location = shift.Location,
            InvolvedPerson = involvedPersonValue,
            Severity = Severity,
            ManpowerCount = manpowerCount
        };

        _context.ShiftLogs.Add(newLog);
        _context.SaveChanges();

        return RedirectToAction("ViewShift", new { id = id });
    }

    [HttpGet]
    public IActionResult AllShifts(string searchTerm, string shiftTypeFilter, string statusFilter)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return Unauthorized();
        }


        var shifts = _context.Shifts
            .ToList(); // ✅ Admin sees ALL (even closed)

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();

            shifts = shifts.Where(s =>
                (s.ShiftType != null && s.ShiftType.ToLower().Contains(searchTerm)) ||
                (s.Location != null && s.Location.ToLower().Contains(searchTerm)) ||
                (s.SupervisorName != null && s.SupervisorName.ToLower().Contains(searchTerm)) ||
                (s.StartTime.ToString("dddd").ToLower().Contains(searchTerm)) ||
                (s.StartTime.ToString("f").ToLower().Contains(searchTerm)) ||
                (s.EndTime.HasValue && s.EndTime.Value.ToString("f").ToLower().Contains(searchTerm)) ||
                (s.IsClaimed ? "claimed".Contains(searchTerm) : "unclaimed".Contains(searchTerm))
            ).ToList();
        }

        if (!string.IsNullOrEmpty(shiftTypeFilter))
        {
            shifts = shifts.Where(s => s.ShiftType != null && s.ShiftType.Equals(shiftTypeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "Claimed")
                shifts = shifts.Where(s => s.IsClaimed).ToList();
            else if (statusFilter == "Unclaimed")
                shifts = shifts.Where(s => !s.IsClaimed).ToList();
        }

        return View("ListAvailableShifts", shifts); // Reuse SAME VIEW
    }




}




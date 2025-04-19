using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShiftHandover.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

//only authenticated users can access this controller
[Authorize]
public class ShiftController : Controller
{
    private readonly ApplicationDbContext _context;

    // Constructor - inject the ApplicationDbContext
    public ShiftController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Shift/GetShifts
    // Returns all claimed shifts of the currently logged-in Supervisor as JSON (for calendar display)
    [HttpGet]
    public IActionResult GetShifts()
    {
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(); // User is not logged in
        }

        var shifts = _context.Shifts
            .Where(s => s.SupervisorName == username && s.IsClaimed) // ✅ Only the shifts claimed by this user
            .Select(s => new
            {
                id = s.Id,
                title = s.ShiftType + " Shift - " + s.Location,
                start = s.StartTime.ToString("s"), // ISO 8601 format
                end = s.EndTime.HasValue ? s.EndTime.Value.ToString("s") : null,
                color = s.IsClosed ? "#6c757d" : "#28a745" // Grey if closed, Green if open
            })
            .ToList();

        return Json(shifts);
    }

    // GET: /Shift/ViewShift/{id}
    // Displays detailed information about a specific shift
    [HttpGet]
    public IActionResult ViewShift(int id)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);
        if (shift == null)
            return NotFound(); // Shift not found

        // Fetch logs related to this shift
        ViewBag.ShiftLogs = _context.ShiftLogs
            .Where(l => l.ShiftId == id)
            .OrderByDescending(l => l.LogTime)
            .ToList();

        // Fetch usernames for dropdown selection (for logging purposes)
        ViewBag.Usernames = _context.Users.Select(u => u.Username).ToList();

        return View(shift);
    }


    // POST: /Shift/CloseShift/{id}
    // Closes a shift (only if the current Supervisor owns it)
    [HttpPost]
    public IActionResult CloseShift(int id)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToAction("Login", "Account"); // Not logged in



        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);

        if (shift == null)
        {
            return NotFound(); // Shift not found
        }

        if (shift.SupervisorName != username || !shift.IsClaimed || shift.IsClosed)
        {
            return Unauthorized(); // Unauthorized action (Protect ownership)
        }

        shift.IsClosed = true; // Close the shift
        _context.SaveChanges();

        return RedirectToAction("ViewShift", new { id = id });
    }

    // GET: /Shift/ListAvailableShifts
    // Lists all available shifts based on role and department, with search and filter options
    [HttpGet]
    public IActionResult ListAvailableShifts(string searchTerm, string shiftTypeFilter, string statusFilter)
    {
        var role = HttpContext.Session.GetString("Role");
        var department = HttpContext.Session.GetString("Department");
        List<Shift> shifts;

        // Check the role: Admin sees all shifts, Supervisor sees only not closed
        if (role == "Admin")
        {
            shifts = _context.Shifts.ToList(); // Admin sees ALL shifts
        }
        else
        {
            // Supervisor sees only available shifts in their department
            var departmentIdStr = HttpContext.Session.GetString("DepartmentId");

            if (string.IsNullOrEmpty(departmentIdStr))
            {
                return Unauthorized();
            }

            int departmentId = int.Parse(departmentIdStr);

            shifts = _context.Shifts
                .Where(s => !s.IsClosed && !s.IsClaimed && s.DepartmentId == departmentId)
                .ToList();


  
            // Only shifts that have NOT started yet (full DateTime comparison)
            var now = DateTime.Now;
            shifts = shifts.Where(s => s.StartTime >= now).ToList();


        }


        // Apply search filter
        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();

            shifts = shifts.Where(s =>
                s.Id.ToString().Contains(searchTerm) || // Shift ID 
                (s.ShiftType != null && s.ShiftType.ToLower().Contains(searchTerm)) || // Shift Type 
                (s.Location != null && s.Location.ToLower().Contains(searchTerm)) || // Location 
                (s.SupervisorName != null && s.SupervisorName.ToLower().Contains(searchTerm)) || // Supervisor 
                (s.StartTime.ToString("g").ToLower().Contains(searchTerm)) || // Start Time 
                (s.EndTime.HasValue && s.EndTime.Value.ToString("g").ToLower().Contains(searchTerm)) || // End Time 
                (s.IsClaimed && "claimed".Contains(searchTerm)) || // Status 
                (!s.IsClaimed && "unclaimed".Contains(searchTerm)) ||
                (s.IsClosed && "closed".Contains(searchTerm))
            ).ToList();
        }



        //  Apply additional filters
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

        // Sort by soonest StartTime
        shifts = shifts.OrderBy(s => s.StartTime).ToList();

        return View(shifts);
    }

    // POST: /Shift/Claim/{id}
    // Claims a shift for the current Supervisor
    [HttpPost]
    public IActionResult Claim(int id)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);

        if (shift == null || shift.IsClaimed)
        {
            TempData["ClaimError"] = "Shift not available for claiming.";
            return RedirectToAction("ListAvailableShifts");
        }

        // Prevent claiming old shifts
        if (shift.StartTime < DateTime.Now)
        {
            TempData["ClaimError"] = "Cannot claim a shift that has already started or ended.";
            return RedirectToAction("ListAvailableShifts");
        }

        var username = HttpContext.Session.GetString("Username");
        if (username == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        //  Check if supervisor already has a shift that overlaps
        var overlappingShift = _context.Shifts
        .Where(s => s.SupervisorName == username && s.IsClaimed && !s.IsClosed)
        .Any(existingShift =>
            (shift.StartTime < existingShift.EndTime && shift.EndTime > existingShift.StartTime)
        );

        if (overlappingShift)
        {
            Console.WriteLine($"Overlap detected for user {username} trying to claim shift {id}");
            return Conflict("You already have a shift that overlaps with this one."); // ⭐ Return 409 Conflict
        }


        //  No conflict, proceed with claim
        shift.IsClaimed = true;
        shift.SupervisorId = user.UserId.ToString();
        shift.SupervisorName = user.Username;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Shift claimed successfully.";
        return RedirectToAction("UserDashboard", "Dashboard");
    }

    // POST: /Shift/GenerateReport/{id}
    // Generates a PDF report for a specific shift using QuestPDF
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

                    // Display shift details
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

                    // Display shift logs in a table
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

                            // Header Row
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Time").SemiBold();
                                header.Cell().Element(CellStyle).Text("Type").SemiBold();
                                header.Cell().Element(CellStyle).Text("Severity").SemiBold();
                                header.Cell().Element(CellStyle).Text("Involved Person").SemiBold();
                                header.Cell().Element(CellStyle).Text("Manpower Count").SemiBold();
                                header.Cell().Element(CellStyle).Text("Description").SemiBold();
                            });

                            // Data Rows
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

        // Helper method to style table cells
        IContainer CellStyle(IContainer container)
        {
            return container.PaddingVertical(5).PaddingHorizontal(2);
        }
    }

    // GET: /Shift/ShiftHistory
    // Shows the Supervisor's past (closed) shifts, with optional search
    [HttpGet]
    public IActionResult ShiftHistory(string searchTerm)
    {
        var username = HttpContext.Session.GetString("Username");
        var role = HttpContext.Session.GetString("Role");

        if (string.IsNullOrEmpty(username))
        {
            // User is not logged in
            return RedirectToAction("Login", "Account");
        }

        if (role != "Supervisor")
        {
            // Only Supervisors can access shift history
            TempData["ErrorMessage"] = "You are not authorized to access Shift History.";
            return RedirectToAction("Login", "Account");
        }

        // Fetch closed shifts for this Supervisor
        var shifts = _context.Shifts
            .Where(s => s.SupervisorName == username && s.IsClosed)
            .OrderByDescending(s => s.StartTime)
            .ToList(); // Load into memory first

        // In-memory search filtering
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

    // POST: /Shift/LogShift
    // Adds a new log (Accident / Incident / Manpower) to a specific shift
    [HttpPost]
    public IActionResult LogShift(int id, string Type, string Description, DateTime LogTime, string InvolvedPerson, string Severity)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);
        if (shift == null)
            return NotFound(); // Shift not found

        var username = HttpContext.Session.GetString("Username");

        // Ensure shift is claimed, open, and belongs to current Supervisor
        if (!shift.IsClaimed || shift.IsClosed || shift.SupervisorName != username)
        {
            TempData["LogError"] = "You are not allowed to log activities for this shift.";
            return RedirectToAction("ViewShift", new { id = id });
        }

        // Ensure log time is within shift timing
        if (LogTime < shift.StartTime.AddSeconds(-1) || (shift.EndTime.HasValue && LogTime > shift.EndTime.Value.AddSeconds(1)))
        {
            TempData["LogError"] = "Log Time must be within your shift's Start and End times.";
            return RedirectToAction("ViewShift", new { id = id });
        }


        // Only allow Accident, Incident, Manpower
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
            // Special handling for manpower logs
            if (!int.TryParse(InvolvedPerson, out int manpower))
            {
                TempData["LogError"] = "Please enter a valid manpower number.";
                return RedirectToAction("ViewShift", new { id = id });
            }

            manpowerCount = manpower;
            involvedPersonValue = null;

            // Update total manpower for the shift
            shift.TotalManpower = (shift.TotalManpower ?? 0) + manpower;
        }
        else
        {
            // Validate involved person exists
            var userExists = _context.Users.Any(u => u.Username == InvolvedPerson);
            if (!userExists)
            {
                TempData["LogError"] = "Involved Person username does not exist.";
                return RedirectToAction("ViewShift", new { id = id });
            }
        }

        // Create a new shift log
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

        if (string.IsNullOrWhiteSpace(Description))
        {
            Description = "No description provided.";
        }

        _context.ShiftLogs.Add(newLog);
        _context.SaveChanges(); // Save new log to database

        return RedirectToAction("ViewShift", new { id = id });
    }

    // GET: /Shift/AllShifts
    // Admins only view that lists all shifts (claimed, unclaimed, closed)
    [HttpGet]
    public IActionResult AllShifts(string searchTerm, string shiftTypeFilter, string statusFilter)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return Unauthorized(); // Only Admin can view all shifts
        }

        // Fetch all shifts with Department info
        var shifts = _context.Shifts.Include(s => s.Department).ToList();// ✅ Admin sees ALL (even closed)

        // Apply search
        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();

            shifts = shifts.Where(s =>
                (s.ShiftType != null && s.ShiftType.ToLower().Contains(searchTerm)) ||
                (s.Location != null && s.Location.ToLower().Contains(searchTerm)) ||
                (s.SupervisorName != null && s.SupervisorName.ToLower().Contains(searchTerm)) ||
                  (s.Department != null && s.Department.DepartmentName.ToLower().Contains(searchTerm)) ||
                (s.StartTime.ToString("dddd").ToLower().Contains(searchTerm)) ||
                (s.StartTime.ToString("f").ToLower().Contains(searchTerm)) ||
                (s.EndTime.HasValue && s.EndTime.Value.ToString("f").ToLower().Contains(searchTerm)) ||
                (s.IsClaimed ? "claimed".Contains(searchTerm) : "unclaimed".Contains(searchTerm))
            ).ToList();
        }

        // Apply shift type filter
        if (!string.IsNullOrEmpty(shiftTypeFilter))
        {
            shifts = shifts.Where(s => s.ShiftType != null && s.ShiftType.Equals(shiftTypeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Apply status filter
        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "Claimed")
                shifts = shifts.Where(s => s.IsClaimed).ToList();
            else if (statusFilter == "Unclaimed")
                shifts = shifts.Where(s => !s.IsClaimed).ToList();
        }

        return View("ListAvailableShifts", shifts); //  Reuse existing view for listing
    }

    // Utility method - Detect shift type based on starting hour
    private string DetectShiftType(DateTime startTime)
    {
        var hour = startTime.Hour;

        if (hour >= 5 && hour < 12)
            return "Morning";
        else if (hour >= 12 && hour < 17)
            return "Afternoon";
        else if (hour >= 17 && hour < 22)
            return "Evening";
        else
            return "Night";
    }

    // GET: /Shift/AddShift
    // Display the Add Shift form
    [HttpGet]
    public IActionResult AddShift()
    {
        return View();
    }

    // POST: /Shift/AddShift
    // Handle form submission to add a new shift
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddShift(Shift model)
    {
        if (!ModelState.IsValid)
        {
            return View(model); // Validation error, redisplay form
        }

        // Set default values for new shift
        model.IsClaimed = false;
        model.IsClosed = false;
        model.SupervisorId = "Unassigned";
        model.SupervisorName = "Unassigned";
        model.TotalManpower = 0; // ✅ ADD THIS LINE!!

        // Detect shift type based on time
        model.ShiftType = DetectShiftType(model.StartTime);

        if (string.IsNullOrEmpty(model.Notes))
        {
            model.Notes = "No notes yet.";
        }

        _context.Shifts.Add(model);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Shift created successfully!";
        return RedirectToAction(nameof(AddShift)); // Redirect back to Add Shift form
    }




}




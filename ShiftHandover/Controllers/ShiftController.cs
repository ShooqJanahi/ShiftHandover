using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShiftHandover.Models;
using System.Linq;

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
        var shifts = _context.Shifts.Select(s => new
        {
            id = s.Id, // Important! Send Id
            title = (s.IsClaimed ? "Claimed by " + s.SupervisorName : "Available Shift") + " - " + s.Location,
            start = s.StartTime.ToString("s"), // ISO 8601
            end = s.EndTime.HasValue ? s.EndTime.Value.ToString("s") : null,
            color = s.IsClaimed ? "#A30020" : "#28a745" // Red if claimed, Green if available
        }).ToList();

        return Json(shifts);
    }

    [HttpGet]
    public IActionResult ViewShift(int id)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);
        if (shift == null)
            return NotFound();

        return View(shift); // Will pass Shift object to the view
    }

    [HttpPost]
    public IActionResult CloseShift(int id)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.Id == id);

        if (shift == null)
        {
            return NotFound();
        }

        shift.IsClosed = true;
        _context.SaveChanges();

        return RedirectToAction("ViewShift", new { id = id });
    }


    [HttpGet]
    public IActionResult ListAvailableShifts(string searchTerm, string shiftTypeFilter, string statusFilter)
    {
        var shifts = _context.Shifts
            .Where(s => !s.IsClosed)
            .ToList(); // Load everything to memory first

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
            .OrderBy(log => log.LogTime)
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
                        innerCol.Item().Text($"Shift Report - {shift.Location}").FontSize(20).Bold();
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
                        foreach (var log in shiftLogs)
                        {
                            col.Item().BorderBottom(1).PaddingVertical(5).Row(row =>
                            {
                                row.RelativeColumn(3).Text($"{log.LogTime:g}");
                                row.RelativeColumn(3).Text(log.Type);
                                row.RelativeColumn(6).Text(log.Description);
                            });
                        }
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
    }


}




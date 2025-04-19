using System;
using System.ComponentModel.DataAnnotations;

namespace ShiftHandover.Models
{
    // Represents a single log entry related to a shift (e.g., incident, manpower, accident, etc.)
    public class ShiftLog
    {
        // Primary key for the ShiftLog table
        public int Id { get; set; }

        // Foreign key to link the log to its associated shift
        [Required]
        public int ShiftId { get; set; }

        // Timestamp when the log was created (defaults to now)
        [Display(Name = "Logged At")]
        public DateTime LogTime { get; set; } = DateTime.Now;

        // Type of log (Accident, Incident, Manpower)
        [Required]
        [StringLength(100)]
        [Display(Name = "Log Type")]
        public string Type { get; set; }

        // Detailed description of the log
        [Required]
        [StringLength(1000)]
        public string? Description { get; set; }

        // Optional: Location where the log event occurred
        [StringLength(100)]
        [Display(Name = "Location (Optional)")]
        public string Location { get; set; }

        // Optional: Severity level of the event (e.g., Low, Medium, High)
        [Display(Name = "Severity Level (Optional)")]
        public string Severity { get; set; } // e.g., Low, Medium, High (useful for accidents/incidents)

        // Optional: Name or ID of any personnel involved in the log
        [Display(Name = "Involved Personnel (Optional)")]
        public string? InvolvedPerson { get; set; } // name or ID of involved employee if any

        // Optional: Number of manpower logged (e.g., for manpower shift updates)
        public int? ManpowerCount { get; set; } // Optional

        // Navigation property - Links this log back to the Shift it belongs to
        public virtual Shift Shift { get; set; }
    }
}

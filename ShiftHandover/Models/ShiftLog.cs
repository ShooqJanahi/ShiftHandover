using System;
using System.ComponentModel.DataAnnotations;

namespace ShiftHandover.Models
{
    public class ShiftLog
    {
        public int Id { get; set; }

        [Required]
        public int ShiftId { get; set; }

        [Display(Name = "Logged At")]
        public DateTime LogTime { get; set; } = DateTime.Now;

        [Required]
        [StringLength(100)]
        [Display(Name = "Log Type")]
        public string Type { get; set; } // e.g. Accident, Incident, Manpower

        [Required]
        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        [Display(Name = "Location (Optional)")]
        public string Location { get; set; }

        [Display(Name = "Severity Level (Optional)")]
        public string Severity { get; set; } // e.g., Low, Medium, High (useful for accidents/incidents)

        [Display(Name = "Involved Personnel (Optional)")]
        public string? InvolvedPerson { get; set; } // name or ID of involved employee if any

        public int? ManpowerCount { get; set; } // Optional


        public virtual Shift Shift { get; set; }
    }
}

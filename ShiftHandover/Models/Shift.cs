using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShiftHandover.Models
{
    // Entity class representing a work Shift
    public class Shift
    {
        // Primary key of the Shift table
        public int Id { get; set; }

        // ID of the supervisor assigned to the shift (stored as string for flexibility)
        public string SupervisorId { get; set; }

        // Name of the supervisor assigned (used for quick display)
        [Display(Name = "Supervisor Name")]
        public string SupervisorName { get; set; }

        // Shift start time
        [Display(Name = "Shift Start Time")]
        public DateTime StartTime { get; set; }

        // Shift end time (nullable because shift may not be ended yet)
        [Display(Name = "Shift End Time")]
        public DateTime? EndTime { get; set; }

        // Indicates whether the shift has been claimed by a supervisor
        [Display(Name = "Shift Status")]
        public bool IsClaimed { get; set; }

        // Indicates whether the shift has been officially closed
        public bool IsClosed { get; set; }

        // Location where the shift takes place
        [Display(Name = "Shift Location")]
        [StringLength(100)]
        public string Location { get; set; }

        // Notes about the shift (optional field)
        [Display(Name = "Notes (Optional)")]
        [StringLength(1000)]
        public string Notes { get; set; }

        // Total manpower reported during the shift (optional)
        [Display(Name = "Total Manpower")]
        public int? TotalManpower { get; set; } // Optional summary field for manpower headcount

        // Type of shift (Morning, Afternoon, Evening, Night, etc.)
        [Display(Name = "Shift Type")]
        public string ShiftType { get; set; } 

        // Foreign key to link the shift to a department
        public int? DepartmentId { get; set; }

        // Navigation property - A shift can have multiple associated logs (accidents, incidents, manpower entries)
        public virtual ICollection<ShiftLog> Logs { get; set; }

        // Navigation property - Link back to the related Department
        public Department Department { get; set; }

    }
}

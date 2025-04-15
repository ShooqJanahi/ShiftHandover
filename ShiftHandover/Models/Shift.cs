using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShiftHandover.Models
{
    public class Shift
    {
        public int Id { get; set; }

        [Required]
        public string SupervisorId { get; set; }

        [Display(Name = "Supervisor Name")]
        public string SupervisorName { get; set; }

        [Display(Name = "Shift Start Time")]
        public DateTime StartTime { get; set; }

        [Display(Name = "Shift End Time")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Shift Status")]
        public bool IsClaimed { get; set; }

        public bool IsClosed { get; set; }

        [Display(Name = "Shift Location")]
        [StringLength(100)]
        public string Location { get; set; }

        [Display(Name = "Notes (Optional)")]
        [StringLength(1000)]
        public string Notes { get; set; }

        [Display(Name = "Total Manpower")]
        public int? TotalManpower { get; set; } // Optional summary field for manpower headcount

        [Display(Name = "Shift Type")]
        public string ShiftType { get; set; } // Morning, Evening, Night, etc.

        public virtual ICollection<ShiftLog> Logs { get; set; }
    }
}

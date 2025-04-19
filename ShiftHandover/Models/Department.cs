using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftHandover.Models
{
    // Department entity representing a department in the organization
    [Table("Departments")] 
    public class Department
    {
        // Primary key for the Department table
        [Key]
        public int DepartmentId { get; set; }

        // Department name field
        [Required(ErrorMessage = "Department name is required.")] // Validation: Must be provided
        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
        public string DepartmentName { get; set; }
    }
}

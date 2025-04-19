using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftHandover.Models
{
    public class User
    {
        // Primary key for the User table
        [Key]
        public int UserId { get; set; }

        // User's first name
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        // User's last name
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        // Unique username for login purposes
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        // Hashed password for security
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } // Makes it clear this is hashed

        // User's email address, validated for proper email format
        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Phone number must be exactly 8 digits.")]
        public string PhoneNumber { get; set; }


        // Optional: foreign key linking to Department entity
        public int? DepartmentId { get; set; }

        // User's role title (e.g., Admin, Supervisor)
        [StringLength(100)]
        public string RoleTitle { get; set; }

        // Indicates if the user account is active; default is true
        public bool IsActive { get; set; } = true;

        // Navigation property for the related Department
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace ShiftHandover.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } // Makes it clear this is hashed

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [StringLength(100)]
        public string RoleTitle { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

using System.ComponentModel.DataAnnotations;

namespace ShiftHandover.Models
{
    // ViewModel representing the login form data
    public class LoginViewModel
    {
        // Username field
        [Required]
        public string Username { get; set; }

        // Password field
        [Required]
        public string Password { get; set; }
    }

}

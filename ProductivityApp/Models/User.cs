using System.ComponentModel.DataAnnotations;

namespace ProductivityApp.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string PasswordHash { get; set; }
    }
}

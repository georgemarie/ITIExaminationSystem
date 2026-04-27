using System.ComponentModel.DataAnnotations;

namespace ITIExaminationSystem.Models
{
    /// <summary>
    /// Represents login credentials (used on the login form).
    /// </summary>
    public class User
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}

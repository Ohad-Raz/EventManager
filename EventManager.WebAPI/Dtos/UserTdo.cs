using System.ComponentModel.DataAnnotations;

namespace EventManager.WebAPI.Dtos
{
    // DTO used for user registration
    public class UserDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters long")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters long")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters long")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters long")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [StringLength(256, ErrorMessage = "Email cannot be longer than 256 characters")]
        [EmailAddress(ErrorMessage = "Provide a valid e-mail address")]
        public string Email { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Phone number cannot be longer than 50 characters")]
        [Phone(ErrorMessage = "Provide a valid phone number")]
        public string? Phone { get; set; }
    }
}
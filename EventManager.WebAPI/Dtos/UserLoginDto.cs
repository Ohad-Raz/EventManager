using System.ComponentModel.DataAnnotations;

namespace EventManager.WebAPI.Dtos
{
    // DTO used only for login
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
    }
}
using System.ComponentModel.DataAnnotations;

namespace EventManager.WebApp.ViewModels
{
    public class LoginVM
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        // Keeps the original page the user wanted before being redirected to login.
        public string? ReturnUrl { get; set; }
    }
}
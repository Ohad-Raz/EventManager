using EventManager.DAL.Validation;
using System.ComponentModel.DataAnnotations;

namespace EventManager.WebAPI.Dtos
{
    // DTO used when logged-in user wants to change password
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 100 characters long")]
        [NotEqualTo(nameof(CurrentPassword), ErrorMessage = "New password must be different from current password")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm the new password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Confirmed password must be between 8 and 100 characters long")]
        [Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation password do not match")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
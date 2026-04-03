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
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm the new password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Confirmed password must be between 8 and 100 characters long")]
        public string ConfirmNewPassword { get; set; } = null!;

        // Helper method for controller validation
        public bool NewPasswordsMatch()
        {
            return NewPassword == ConfirmNewPassword;
        }

        // prevent user from reusing the same password
        public bool IsDifferentFromCurrent()
        {
            return NewPassword != CurrentPassword;
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace EventManager.WebApp.ViewModels
{
    public class UpdateProfileVM
    {
        [Required]
        [StringLength(256)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(256)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Phone number cannot be longer than 50 characters")]
        [Phone(ErrorMessage = "Phone number format is invalid")]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }
    }
}

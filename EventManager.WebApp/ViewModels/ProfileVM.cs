using System.ComponentModel.DataAnnotations;

namespace EventManager.WebApp.ViewModels
{
    public class ProfileVM
    {
        [Display(Name = "Username")]
        public string Username { get; set; } = null!;

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

        [Phone]
        [StringLength(256)]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Display(Name = "Role")]
        public string RoleName { get; set; } = null!;
    }
}

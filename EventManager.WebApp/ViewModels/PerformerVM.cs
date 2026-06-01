using System.ComponentModel.DataAnnotations;

namespace EventManager.WebApp.ViewModels
{
    public class PerformerVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Performer Name")]
        public string Name { get; set; } = null!;

        [StringLength(1000)]
        [Display(Name = "Biography")]
        public string? Bio { get; set; }
    }
}

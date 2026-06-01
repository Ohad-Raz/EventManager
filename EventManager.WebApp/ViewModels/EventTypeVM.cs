using System.ComponentModel.DataAnnotations;

namespace EventManager.WebApp.ViewModels
{
    public class EventTypeVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Event Type Name")]
        public string Name { get; set; } = null!;
    }
}

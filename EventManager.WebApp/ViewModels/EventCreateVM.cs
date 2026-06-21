using EventManager.DAL.Validation;
using System.ComponentModel.DataAnnotations;

namespace EventManager.WebApp.ViewModels
{
    public class EventCreateVM
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Event Name")]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!;

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        [DateGreaterThan(nameof(StartTime), ErrorMessage = "End time must be later than start time.")]
        public DateTime EndTime { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Event Type")]
        public int EventTypeId { get; set; }

        [Display(Name = "Image")]
        public int? ImageId { get; set; }
    }
}

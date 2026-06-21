using EventManager.DAL.Validation;
using System.ComponentModel.DataAnnotations;

namespace EventManager.WebAPI.Dtos
{
    public class EventDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        [DateGreaterThan(nameof(StartTime), ErrorMessage = "End time must be later than start time.")]
        public DateTime EndTime { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        [Range(1, int.MaxValue)]
        public int EventTypeId { get; set; }

        public int? ImageId { get; set; }
    
    }
}
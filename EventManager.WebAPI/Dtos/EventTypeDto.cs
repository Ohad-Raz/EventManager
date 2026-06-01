using System.ComponentModel.DataAnnotations;

namespace EventManager.WebAPI.Dtos
{
    public class EventTypeDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
    }
}

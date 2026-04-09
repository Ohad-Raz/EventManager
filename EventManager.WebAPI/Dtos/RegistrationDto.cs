using System.ComponentModel.DataAnnotations;

namespace EventManager.WebAPI.Dtos
{
    public class RegistrationDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int EventId { get; set; }
    }
}

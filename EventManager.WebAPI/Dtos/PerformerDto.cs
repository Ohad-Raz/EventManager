using System.ComponentModel.DataAnnotations;

namespace EventManager.WebAPI.Dtos
{
    public class PerformerDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        [StringLength(1000)]
        public string? Bio { get; set; }
    }
}


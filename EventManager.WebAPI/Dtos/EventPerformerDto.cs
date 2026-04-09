using System.ComponentModel.DataAnnotations;

namespace EventManager.WebAPI.Dtos
{
    public class EventPerformerDto
    {
        [Range(1, int.MaxValue)]
        public int PerformerId { get; set; }
    }
}
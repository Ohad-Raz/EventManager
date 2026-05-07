using System.ComponentModel.DataAnnotations;

namespace EventManager.WebApp.ViewModels
{
    public class EventVM
    {
        public int Id { get; set; }

        [Display(Name = "Event Name")]
        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        public string Location { get; set; } = null!;

        public int Capacity { get; set; }

        [Display(Name = "Event Type")]
        public int EventTypeId { get; set; }

        [Display(Name = "Event Type")]
        public string EventTypeName { get; set; } = null!;

        public int CreatedById { get; set; }

        [Display(Name = "Created By")]
        public string CreatedByEmail { get; set; } = null!;

        [Display(Name = "Image")]
        public int? ImageId { get; set; }

        [Display(Name = "Image")]
        public string ImageFileName { get; set; } = null!;
    }
}

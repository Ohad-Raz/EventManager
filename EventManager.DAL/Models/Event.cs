using System;
using System.Collections.Generic;

namespace EventManager.DAL.Models;

public partial class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Location { get; set; } = null!;

    public int Capacity { get; set; }

    public int EventTypeId { get; set; }

    public int CreatedById { get; set; }

    public int? ImageId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User CreatedBy { get; set; } = null!;

    public virtual ICollection<EventPerformer> EventPerformers { get; set; } = new List<EventPerformer>();

    public virtual EventType EventType { get; set; } = null!;

    public virtual Image? Image { get; set; }

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}

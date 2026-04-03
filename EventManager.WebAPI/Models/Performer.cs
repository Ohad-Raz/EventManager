using System;
using System.Collections.Generic;

namespace EventManager.WebAPI.Models;

public partial class Performer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Bio { get; set; }

    public virtual ICollection<EventPerformer> EventPerformers { get; set; } = new List<EventPerformer>();
}

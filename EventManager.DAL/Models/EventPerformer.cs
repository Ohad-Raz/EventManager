using System;
using System.Collections.Generic;

namespace EventManager.DAL.Models;

public partial class EventPerformer
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int PerformerId { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Performer Performer { get; set; } = null!;
}

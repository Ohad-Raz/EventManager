using System;
using System.Collections.Generic;

namespace EventManager.WebAPI.Models;

public partial class Registration
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int UserId { get; set; }

    public int EventId { get; set; }

    public DateTime RegisteredAt { get; set; }

    public bool IsActive { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace EventManager.DAL.Models;

public partial class Log
{
    public int Id { get; set; }

    public DateTime Timestamp { get; set; }

    public int Level { get; set; }

    public string Message { get; set; } = null!;

    public string? ErrorText { get; set; }
}

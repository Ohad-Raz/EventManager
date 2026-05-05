using System;
using System.Collections.Generic;

namespace EventManager.DAL.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PwdHash { get; set; } = null!;

    public string PwdSalt { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual Role Role { get; set; } = null!;
}

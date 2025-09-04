using System;
using System.Collections.Generic;

namespace WebAPI_to_MySQL.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string? Auth0UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public sbyte IsActive { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

using System;
using System.Collections.Generic;

namespace WebAPI_to_MySQL.Entities;

public partial class PaymentStatus
{
    public int PaymentStatusId { get; set; }

    public string StatusCode { get; set; } = null!;

    public string? StatusDescription { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

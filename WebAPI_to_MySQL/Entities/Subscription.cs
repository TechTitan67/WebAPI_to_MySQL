using System;
using System.Collections.Generic;

namespace WebAPI_to_MySQL.Entities;

public partial class Subscription
{
    public int SubscriptionId { get; set; }

    public int UserId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? LastPaymentDate { get; set; }

    public int? PaymentStatusId { get; set; }

    public virtual PaymentStatus? PaymentStatus { get; set; }

    public virtual User User { get; set; } = null!;
}

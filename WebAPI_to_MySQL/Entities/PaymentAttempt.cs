using System;
using System.Collections.Generic;

namespace WebAPI_to_MySQL.Entities;

public partial class PaymentAttempt
{
    public int PaymentAttemptId { get; set; }

    public int UserId { get; set; }

    public string PaymentId { get; set; } = null!;

    public DateTime AttemptedAt { get; set; }

    public bool IsSuccessful { get; set; }

    public string? FailureReason { get; set; }

    public decimal? Amount { get; set; }

    public string? RawResponse { get; set; }

    public virtual User User { get; set; } = null!;
}

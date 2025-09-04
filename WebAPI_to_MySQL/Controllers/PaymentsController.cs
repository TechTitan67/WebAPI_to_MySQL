using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI_to_MySQL.Entities;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly NeurotechnexusContext _context;

    public PaymentsController(IPaymentService paymentService, NeurotechnexusContext context)
    {
        _paymentService = paymentService;
        _context = context;
    }

    [Authorize]
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] string paymentId)
    {
        var isValid = await _paymentService.VerifyPaymentAsync(paymentId);

        // Get user ID from JWT claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("User ID not found in token.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            return BadRequest("User not found.");

        // Log the payment attempt (before any subscription logic)
        var paymentAttempt = new PaymentAttempt
        {
            UserId = user.UserId,
            PaymentId = paymentId,
            AttemptedAt = DateTime.UtcNow,
            IsSuccessful = isValid,
            FailureReason = isValid ? null : "Payment not verified"
        };
        _context.Set<PaymentAttempt>().Add(paymentAttempt);
        await _context.SaveChangesAsync();

        if (!isValid)
            return BadRequest("Payment not verified.");

        // Check for an active subscription
        var activeSubscription = await _context.Subscriptions
    .FirstOrDefaultAsync(s => s.UserId == user.UserId && s.IsActive == true);

        if (activeSubscription == null)
        {
            var activeStatus = await _context.Set<PaymentStatus>()
                .FirstOrDefaultAsync(ps => ps.StatusCode == "Active");
            if (activeStatus == null)
                return BadRequest("Active payment status not found.");

            var newSubscription = new Subscription
            {
                UserId = user.UserId,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                LastPaymentDate = DateTime.UtcNow,
                PaymentStatusId = activeStatus.PaymentStatusId
            };
            _context.Subscriptions.Add(newSubscription);
        }
        else
        {
            activeSubscription.LastPaymentDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok();
    }
}
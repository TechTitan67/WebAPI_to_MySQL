using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI_to_MySQL.DTOs;
using WebAPI_to_MySQL.Entities;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly NeurotechnexusContext _context;

    public SubscriptionsController(NeurotechnexusContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Subscription>>> GetSubscriptions()
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Subscription>> GetSubscription(int id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null)
            return NotFound();
        return subscription;
    }

    [HttpPost]
    public async Task<ActionResult<Subscription>> PostSubscription(SubscriptionCreateDto dto)
    {
        var subscription = new Subscription
        {
            UserId = dto.UserID,
            IsActive = dto.IsActive,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            LastPaymentDate = dto.LastPaymentDate,
            PaymentStatusId = dto.PaymentStatusID // <-- assign the FK, not the navigation property
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubscription), new { id = subscription.SubscriptionId }, subscription);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutSubscription(int id, Subscription subscription)
    {
        if (id != subscription.SubscriptionId)
            return BadRequest();

        _context.Entry(subscription).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null)
            return NotFound();

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
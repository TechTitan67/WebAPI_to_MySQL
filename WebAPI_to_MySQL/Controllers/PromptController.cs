namespace WebAPI_to_MySQL.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;
    using WebAPI_to_MySQL.Entities;

    [ApiController]
    [Route("api/[controller]")]
    public class PromptController : ControllerBase
    {
        private readonly NeurotechnexusContext _dbContext; // Rename field to avoid ambiguity

        public PromptController(NeurotechnexusContext dbContext) // Update parameter name to match renamed field
        {
            _dbContext = dbContext;
        }

        // Restrict to Admins or active paid subscribers
        [Authorize]
        [HttpPost("build")]
        public async Task<IActionResult> BuildPrompt([FromBody] string promptInput)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null)
                return Unauthorized();

            if (User.IsInRole("Admin"))
            {
                // Allow Admins
                return Ok(new { prompt = promptInput, message = "Prompt built (Admin access granted)." });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var hasActiveSubscription = await _dbContext.Subscriptions
                .AnyAsync(s => s.UserId == userId && s.IsActive == true);

            if (hasActiveSubscription)
            {
                // Allow active subscribers
                return Ok(new { prompt = promptInput, message = "Prompt built (Subscriber access granted)." });
            }

            return Forbid();
        }

        // Rename the duplicate method to resolve CS0111
        [Authorize]
        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePrompt([FromBody] string promptInput)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null)
                return Unauthorized();

            if (User.IsInRole("Admin"))
            {
                // Allow Admins
                return Ok(new { prompt = promptInput, message = "Prompt generated (Admin access granted)." });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var hasActiveSubscription = await _dbContext.Subscriptions
                .AnyAsync(s => s.UserId == userId && s.IsActive == true);

            if (hasActiveSubscription)
            {
                // Allow active subscribers
                return Ok(new { prompt = promptInput, message = "Prompt generated (Subscriber access granted)." });
            }

            return Forbid();
        }
    }
}
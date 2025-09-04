using Microsoft.AspNetCore.Authorization;       // using Microsoft.AspNetCore.Http;
using System.Security.Claims;                   // using System.Threading.Tasks; // using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;            // using System.Linq; // using System
using Microsoft.AspNetCore.Mvc;                 // using System; // using System.Threading.Tasks; // using System.Collections.Generic; // using System.Linq; // using System.Security.Claims;
using Microsoft.EntityFrameworkCore;            // using System.Threading.Tasks; // using System.Collections.Generic; // using System.Linq; // using System.Security.Claims;
using WebAPI_to_MySQL.DTOs;                     // using WebAPI_to_MySQL.Entities; // using WebAPI_to_MySQL.DTOs
using WebAPI_to_MySQL.Entities;                 // using WebAPI_to_MySQL.Entities; // using WebAPI_to_MySQL.DTOs

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly NeurotechnexusContext _context;
    private readonly IUserStatusService _userStatusService;

    public UsersController(NeurotechnexusContext context, IUserStatusService userStatusService)
    {
        _context = context;
        _userStatusService = userStatusService;
    }

    [Authorize]
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var auth0UserId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        User? user = null;

        if (!string.IsNullOrEmpty(auth0UserId))
        {
            user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.PaymentStatus)
                .FirstOrDefaultAsync(u => u.Auth0UserId == auth0UserId);
        }

        if (user == null && !string.IsNullOrEmpty(email))
        {
            user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.PaymentStatus)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        if (user == null)
            return NotFound();

        var (isAdmin, isActiveSubscriber) = await _userStatusService.GetUserStatusAsync(user);

        return Ok(new
        {
            IsAdmin = isAdmin,
            IsActiveSubscriber = isActiveSubscriber
        });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Select(u => new UserDto
            {
                UserID = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                RoleName = u.Role.RoleName ?? string.Empty, // Fix: Handle possible null reference
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive != 0, // Fix: Convert sbyte to bool
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return users;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> GetCurrentUser()
    {
        var auth0UserId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        User? user = null;

        if (!string.IsNullOrEmpty(auth0UserId))
        {
            user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.PaymentStatus)
                .FirstOrDefaultAsync(u => u.Auth0UserId == auth0UserId);
        }

        if (user == null && !string.IsNullOrEmpty(email))
        {
            user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.PaymentStatus)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // Add this block to check for NameIdentifier
        if (user == null && !string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
        {
            user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.PaymentStatus)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        if (user == null)
            return NotFound();

        var (isAdmin, isActiveSubscriber) = await _userStatusService.GetUserStatusAsync(user);

        var result = new
        {
            UserID = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            RoleName = user.Role.RoleName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive != 0,
            LastLoginAt = user.LastLoginAt,
            IsAdmin = isAdmin,
            IsActiveSubscriber = isActiveSubscriber
        };

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.UserId == id)
            .Select(u => new UserDto
            {
                UserID = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                RoleName = u.Role.RoleName ?? string.Empty, // Fix: Handle possible null reference
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive != 0, // Fix: Convert sbyte to bool
                LastLoginAt = u.LastLoginAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return user;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> PostUser(UserCreateDto userDto)
    {
        // Hash the password before saving
        var user = new User
        {
            UserName = userDto.UserName,
            Email = userDto.Email,
            CreatedAt = userDto.CreatedAt,
            IsActive = (sbyte)(userDto.IsActive ? 1 : 0), // Fix: Convert bool to sbyte explicitly
            FullName = userDto.FullName,
            PhoneNumber = userDto.PhoneNumber,
            LastLoginAt = userDto.LastLoginAt,
            RoleId = userDto.RoleID
        };

        // Use PasswordHasher<User> to hash the password
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, userDto.PasswordHash);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Add Subscription Creation to PostUser
        if (user.RoleId == 2) // 2 = Subscriber
        {
            var activeStatus = await _context.Set<PaymentStatus>() // Fix: Use Set<PaymentStatus>() to access the PaymentStatuses DbSet
                .FirstOrDefaultAsync(ps => ps.StatusCode == "Active");

            if (activeStatus == null)
                return BadRequest("Active payment status not found.");

            var subscription = new Subscription
            {
                UserId = user.UserId,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = null,
                LastPaymentDate = null,
                PaymentStatusId = activeStatus.PaymentStatusId
            };
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
        }

        var result = new UserDto
        {
            UserID = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            RoleName = (await _context.Set<Role>().FindAsync(user.RoleId))?.RoleName ?? "",
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive != 0, // Fix: Convert sbyte to bool
            LastLoginAt = user.LastLoginAt
        };

        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, result);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, User user)
    {
        if (id != user.UserId)
            return BadRequest();

        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Set<User>().FindAsync(id); // Use Set<User>() to access the Users DbSet
        if (user == null)
            return NotFound();

        _context.Set<User>().Remove(user); // Use Set<User>() to remove the user
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
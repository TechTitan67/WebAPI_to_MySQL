using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI_to_MySQL.DTOs;
using WebAPI_to_MySQL.Entities; // Needed for DbContext1 - Don't delete

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly NeurotechnexusContext _context;
    private readonly IConfiguration _config;

    public AuthController(NeurotechnexusContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto loginDto)
    {
        // Find user by username
        var user = _context.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.UserName == loginDto.UserName);

        if (user == null || user.Role?.RoleName == null)
            return Unauthorized();

        // Verify password using PasswordHasher<User>
        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
        if (result != PasswordVerificationResult.Success)
            return Unauthorized();

        // Create claims including role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.RoleName) // RoleName is now guaranteed to be non-null
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
    
    [HttpGet("status/{userId}")]
    public async Task<IActionResult> GetUserStatus(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Subscriptions)
                .ThenInclude(s => s.PaymentStatus)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return NotFound();

        bool isAdmin = user.Role.RoleName == "Admin";
        bool isActiveSubscriber = user.Subscriptions.Any(s => s.IsActive == true && s.PaymentStatus != null && s.PaymentStatus.StatusCode == "Active");

        return Ok(new
        {
            IsAdmin = isAdmin,
            IsActiveSubscriber = isActiveSubscriber
        });
    }
}
namespace WebAPI_to_MySQL.DTOs
{
    public class UserCreateDto
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int RoleID { get; set; }
    }

}

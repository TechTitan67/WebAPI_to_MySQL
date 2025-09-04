using WebAPI_to_MySQL.Entities;

public interface IUserStatusService
{
    Task<(bool IsAdmin, bool IsActiveSubscriber)> GetUserStatusAsync(User user);
}

public class UserStatusService : IUserStatusService
{
    public Task<(bool IsAdmin, bool IsActiveSubscriber)> GetUserStatusAsync(User user)
    {
        bool isAdmin = user.Role?.RoleName == "Admin";
        bool isActiveSubscriber = user.Subscriptions?.Any(s => (s.IsActive ?? false) && s.PaymentStatus?.StatusCode == "Active") ?? false;
        return Task.FromResult((isAdmin, isActiveSubscriber));
    }
}
using Microsoft.EntityFrameworkCore;
using WebAPI_to_MySQL.Entities;

public class DbContext1 : DbContext
{
    public DbContext1(DbContextOptions<DbContext1> options) : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Criterion> Criteria { get; set; }

    // Role-based Access Control
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<PaymentStatus> PaymentStatuses { get; set; }
    public DbSet<PaymentAttempt> PaymentAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>().ToTable("category");
        modelBuilder.Entity<Project>().ToTable("project");
        modelBuilder.Entity<Criterion>().ToTable("criterion");
        modelBuilder.Entity<PaymentAttempt>().ToTable("payment_attempt");
        modelBuilder.Entity<Role>().ToTable("role");
        modelBuilder.Entity<User>().ToTable("user");
        modelBuilder.Entity<Subscription>().ToTable("subscription");
        modelBuilder.Entity<PaymentStatus>().ToTable("payment_status");

        // User -> Role (many-to-one)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleID)
            .OnDelete(DeleteBehavior.Restrict);

        // Subscription -> User (many-to-one)
        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


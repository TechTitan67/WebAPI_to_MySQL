using Microsoft.EntityFrameworkCore;

namespace WebAPI_to_MySQL.Entities
{
    public class NeurotechnexusContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Criterion> Criteria { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Role> Roles { get; set; } // Add this property to define the Roles DbSet
        public DbSet<Subscription> Subscriptions { get; set; } // <-- Add this line
        public DbSet<User> User { get; set; } // Change from Users to User to match the table name
        public DbSet<User> Users { get; set; }

        public NeurotechnexusContext(DbContextOptions<NeurotechnexusContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add any specific configurations for your entities here
            // Additional configuration for the Role entity can be added here if needed
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.SubscriptionId);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Subscriptions)
                      .HasForeignKey(e => e.UserId);
                entity.HasKey(e => e.SubscriptionId);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired(false);
                entity.Property(e => e.LastPaymentDate).IsRequired(false);
                entity.Property(e => e.PaymentStatusId).IsRequired();
            });
            modelBuilder.Entity<Category>().ToTable("category");
            modelBuilder.Entity<Criterion>().ToTable("criterion");
            modelBuilder.Entity<PaymentAttempt>().ToTable("payment_attempt");
            modelBuilder.Entity<PaymentStatus>().ToTable("payment_status");
            modelBuilder.Entity<Project>().ToTable("project");
            modelBuilder.Entity<Role>().ToTable("role");
            modelBuilder.Entity<Subscription>().ToTable("subscription");
            modelBuilder.Entity<User>().ToTable("user");
        }
    }
}
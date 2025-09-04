public class DbContext1 : DbContext
{
    public DbContext1(DbContextOptions<DbContext1> options) : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Criterion> Criteria { get; set; }
}

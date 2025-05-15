using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

    /// <summary>
    /// 角色
    /// </summary>
    public DbSet<Role> Roles { get; set; }
    
    /// <summary>
    /// 後台人員
    /// </summary>
    public DbSet<User> Staffs { get; set; }

    /// <summary>
    /// 可以訪問資源的 Applications( backend servers)
    /// </summary>
    public DbSet<App> Apps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var config = new DbConfig();
        modelBuilder.ApplyConfiguration<Role>(config);
        modelBuilder.ApplyConfiguration<User>(config);
        modelBuilder.ApplyConfiguration<App>(config);
        base.OnModelCreating(modelBuilder);
    }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToUtcConverter>();
    }
}
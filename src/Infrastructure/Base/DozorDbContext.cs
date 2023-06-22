using System.Reflection;
using DozorBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DozorBot.Infrastructure.Base;

public sealed class DozorDbContext : DbContext
{
    public DbSet<TelegramMessage> TgMessages { get; set; }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<AspNetUser> AspNetUsers { get; set; }
    public DbSet<Settings> Settings { get; set; }

    public DozorDbContext(DbContextOptions<DozorDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
        builder.Entity<AppUser>()
            .HasOne(u => u.LegacyUser)
            .WithMany()
            .HasForeignKey(u => u.Id)
            .HasConstraintName("fk_aspnet_users_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
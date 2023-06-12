using DozorBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DozorBot.Infrastructure.Base;

public class DepoDbContext : DbContext
{
    public DbSet<TelegramMessage> TgMessages { get; set; }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<AspNetUser> AspNetUsers { get; set; }

    public DepoDbContext(DbContextOptions<DepoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
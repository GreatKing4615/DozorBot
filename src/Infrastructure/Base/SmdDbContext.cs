using DozorBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DozorBot.Infrastructure.Base;

public class SmdDbContext : DbContext
{
    public DbSet<TelegramMessage> TgMessages { get; set; }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<AspNetUser> AspNetUsers { get; set; }

    public SmdDbContext(DbContextOptions<SmdDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
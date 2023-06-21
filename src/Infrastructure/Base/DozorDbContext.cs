using System.Reflection;
using DozorBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DozorBot.Infrastructure.Base;

public class DozorDbContext : DbContext
{
    public DbSet<TelegramMessage> TgMessages { get; set; }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<AspNetUser> AspNetUsers { get; set; }
    public DbSet<Settings> Settings { get; set; }

    public DozorDbContext(DbContextOptions<DozorDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
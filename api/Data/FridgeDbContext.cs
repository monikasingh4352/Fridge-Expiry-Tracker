using FridgeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FridgeApi.Data;

// Entity Framework Core connects the C# classes to the SQLite database.
public class FridgeDbContext(DbContextOptions<FridgeDbContext> options) : DbContext(options)
{
    public DbSet<FoodItem> FoodItems => Set<FoodItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FoodItem>(e =>
        {
            e.Property(i => i.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
        });
    }
}

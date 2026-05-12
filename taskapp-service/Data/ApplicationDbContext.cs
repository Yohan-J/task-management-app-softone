using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<NewTaskItem> NewTaskItems => Set<NewTaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Title)
            .HasMaxLength(120);

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Description)
            .HasMaxLength(500);

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Priority)
            .HasMaxLength(20);

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}

using Microsoft.EntityFrameworkCore;
using TaskService.Domain.Entities;

namespace TaskService.Infrastructure.Persistence;

public class TasksDbContext(DbContextOptions<TasksDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(c => new { c.UserId, c.Name }).IsUnique();
        });

        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).HasMaxLength(200).IsRequired();
            e.Property(t => t.Description).HasMaxLength(2000);
            e.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);
            e.Property(t => t.Tags).HasColumnType("json");
            e.HasOne(t => t.Category)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

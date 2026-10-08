using Microsoft.EntityFrameworkCore;
using Training.ImageSystem.Models;

namespace Training.ImageSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<ImageJob> ImageJobs => Set<ImageJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.Username)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<ImageJob>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.InputPath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.OutputPath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.ErrorCode)
                .HasMaxLength(100);

            entity.Property(x => x.ErrorMessage)
                .HasMaxLength(1000);

            entity.Property(x => x.RowVersion)
                .IsRowVersion();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
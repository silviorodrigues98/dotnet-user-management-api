using DotnetUserManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetUserManagementApi.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Name)
                .HasMaxLength(User.MaxNameLength)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(254)
                .IsRequired();

            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.PasswordHash)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(u => u.CreatedAtUtc).IsRequired();
        });
    }
}
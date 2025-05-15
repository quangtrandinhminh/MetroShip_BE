using MetroShip.Repository.Models;
using MetroShip.Repository.Models.Identity;
using MetroShip.Utility.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MetroShip.Repository.Infrastructure;

public sealed partial class AppDbContext : IdentityDbContext<UserEntity, RoleEntity, string>
{
    private const int CommandTimeoutInSecond = 20 * 60;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Database.SetCommandTimeout(CommandTimeoutInSecond);
    }

    public AppDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(b =>
        {
            // Each User can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        // create index
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.Email);

        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.PhoneNumber);

        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.CreatedTime);


        modelBuilder.Entity<RoleEntity>(b =>
        {
            // Each Role can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName.StartsWith("AspNet"))
            {
                entityType.SetTableName(tableName.Substring(6));
            }
        };

        modelBuilder.Entity<Route>(b =>
        {
            b.HasOne(r => r.FromStation)
                .WithMany(s => s.RoutesFrom)
                .HasForeignKey(r => r.FromStationId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(r => r.ToStation)
                .WithMany(s => s.RoutesTo)
                .HasForeignKey(r => r.ToStationId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(r => r.MetroLine)
                .WithMany(l => l.Routes)
                .HasForeignKey(r => r.LineId);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed data for roles, get roles from enum
        var roles = Enum.GetValues(typeof(UserRoleEnum))
            .Cast<UserRoleEnum>()
            .Select(role => new RoleEntity
            {
                Id = Guid.NewGuid().ToString(),
                Name = role.ToString(),
                NormalizedName = role.ToString().ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            })
            .ToList();

        modelBuilder.Entity<RoleEntity>().HasData(roles);
    }
}
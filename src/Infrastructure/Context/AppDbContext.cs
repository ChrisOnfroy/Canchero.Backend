using Domain.Entities;
using Infrastructure.Configurations.Converters;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

/// <summary>
/// Contexto de persistencia de Identity y del dominio Canchero.
/// </summary>
public sealed class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> UserProfiles => Set<User>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("identity_users");
        builder.Entity<IdentityRole<Guid>>().ToTable("identity_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("identity_user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("identity_user_logins");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("identity_user_roles");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("identity_user_tokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("identity_role_claims");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ApplyUtcDateTimeOffsetConverters(builder);
    }

    private static void ApplyUtcDateTimeOffsetConverters(ModelBuilder builder)
    {
        var converter = new UtcDateTimeOffsetConverter();
        foreach (var property in builder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            var propertyType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
            if (propertyType == typeof(DateTimeOffset))
            {
                property.SetValueConverter(converter);
            }
        }
    }
}

using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Configurations;

/// <summary>
/// Configura la persistencia de canchas y sus relaciones.
/// </summary>
public sealed class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        builder.ToTable("courts");
        builder.HasKey(court => court.Id);
        builder.Property(court => court.Id).ValueGeneratedNever();
        builder.Property(court => court.CreatedAt).HasColumnName("created_at");
        builder.Property(court => court.UpdatedAt).HasColumnName("updated_at");
        builder.Property(court => court.VenueId).HasColumnName("venue_id").IsRequired();
        builder.HasIndex(court => court.VenueId);
        builder.HasOne<Venue>().WithMany().HasForeignKey(court => court.VenueId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(court => court.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(court => court.SportType).HasConversion<string>().HasColumnName("sport_type").IsRequired();
        builder.HasIndex(court => court.SportType);
        builder.Property(court => court.SurfaceType).HasConversion<string>().HasColumnName("surface_type").IsRequired();
        builder.Property(court => court.Capacity)
            .HasConversion(new ValueConverter<Capacity, int>(
                capacity => capacity.Players,
                value => new Capacity(value)))
            .HasColumnName("capacity")
            .IsRequired();
        builder.OwnsOne(court => court.PricePerHour, money =>
        {
            money.Property(value => value.Amount).HasColumnName("price_per_hour_amount").IsRequired();
            money.Property(value => value.Currency).HasConversion<string>()
                .HasColumnName("price_per_hour_currency").IsRequired();
        });
        builder.Property(court => court.IsIndoor).HasColumnName("is_indoor").IsRequired();
        builder.Property(court => court.Status).HasConversion<string>().HasColumnName("status").IsRequired();
    }
}

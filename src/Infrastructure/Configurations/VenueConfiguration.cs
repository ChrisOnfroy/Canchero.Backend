using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Configurations;

/// <summary>
/// Configura la persistencia de sedes y sus relaciones.
/// </summary>
public sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues");
        builder.HasKey(venue => venue.Id);
        builder.Property(venue => venue.Id).ValueGeneratedNever();
        builder.Property(venue => venue.CreatedAt).HasColumnName("created_at");
        builder.Property(venue => venue.UpdatedAt).HasColumnName("updated_at");
        builder.Property(venue => venue.OwnerId).HasColumnName("owner_id").IsRequired();
        builder.HasIndex(venue => venue.OwnerId);
        builder.HasOne<User>().WithMany().HasForeignKey(venue => venue.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(venue => venue.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(venue => venue.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.OwnsOne(venue => venue.Address, address =>
        {
            address.Property(value => value.Street).HasColumnName("address_street").IsRequired();
            address.Property(value => value.City).HasColumnName("address_city").IsRequired();
            address.Property(value => value.Department).HasColumnName("address_department").IsRequired();
            address.Property(value => value.Country).HasColumnName("address_country").IsRequired();
        });
        builder.OwnsOne(venue => venue.Coordinates, coordinates =>
        {
            coordinates.Property(value => value.Latitude).HasColumnName("latitude").IsRequired();
            coordinates.Property(value => value.Longitude).HasColumnName("longitude").IsRequired();
        });
        builder.Property(venue => venue.PhoneNumber)
            .HasConversion(new ValueConverter<PhoneNumber, string>(
                phone => phone.Value,
                value => new PhoneNumber(value)))
            .HasColumnName("phone_number")
            .IsRequired();
        builder.OwnsOne(venue => venue.OpeningTime, time =>
        {
            time.Property(value => value.Hour).HasColumnName("opening_hour").IsRequired();
            time.Property(value => value.Minute).HasColumnName("opening_minute").IsRequired();
        });
        builder.OwnsOne(venue => venue.ClosingTime, time =>
        {
            time.Property(value => value.Hour).HasColumnName("closing_hour").IsRequired();
            time.Property(value => value.Minute).HasColumnName("closing_minute").IsRequired();
        });
        builder.Property(venue => venue.TimeZoneId).HasColumnName("time_zone_id").HasMaxLength(64).IsRequired();
        builder.Property(venue => venue.IsActive).HasColumnName("is_active").IsRequired();
    }
}

using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

/// <summary>
/// Configura la persistencia de reservas y sus relaciones.
/// </summary>
public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(reservation => reservation.Id);
        builder.Property(reservation => reservation.Id).ValueGeneratedNever();
        builder.Property(reservation => reservation.CreatedAt).HasColumnName("created_at");
        builder.Property(reservation => reservation.UpdatedAt).HasColumnName("updated_at");
        builder.Property(reservation => reservation.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(reservation => reservation.CourtId).HasColumnName("court_id").IsRequired();
        builder.HasIndex(reservation => reservation.UserId);
        builder.HasIndex(reservation => reservation.CourtId);
        builder.HasIndex(reservation => reservation.Status);
        builder.HasOne<User>().WithMany().HasForeignKey(reservation => reservation.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Court>().WithMany().HasForeignKey(reservation => reservation.CourtId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.OwnsOne(reservation => reservation.TimeSlot, slot =>
        {
            slot.Property(value => value.Start).HasColumnName("slot_start").IsRequired();
            slot.Property(value => value.End).HasColumnName("slot_end").IsRequired();
        });
        builder.OwnsOne(reservation => reservation.TotalPrice, money =>
        {
            money.Property(value => value.Amount).HasColumnName("total_price_amount").IsRequired();
            money.Property(value => value.Currency).HasConversion<string>()
                .HasColumnName("total_price_currency").IsRequired();
        });
        builder.Property(reservation => reservation.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .IsRequired();
        builder.Property(reservation => reservation.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(300);
    }
}

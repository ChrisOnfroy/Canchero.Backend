using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using Domain.ValueObjects;

namespace Canchero.Domain.UnitTest;

public sealed class VenueAndReservationRuleTests
{
    private static Venue CreateVenue() => new(Guid.NewGuid(), "Sede", "Demo",
        new Address("Calle 1", "Barranquilla", "Atlántico", "Colombia"),
        new Coordinates(10.99, -74.80), new PhoneNumber("+573001234567"),
        new TimeOfDay(8, 0), new TimeOfDay(22, 0));

    [Fact]
    public void Venue_UsesBogotaTimezoneAndRejectsInvalidSchedule()
    {
        var venue = CreateVenue();
        Assert.Equal("America/Bogota", venue.TimeZoneId);
        Assert.Throws<BusinessRuleException>(() => new Venue(Guid.NewGuid(), "Mal", "",
            new Address("Calle 1", "Barranquilla", "Atlántico", "Colombia"),
            new Coordinates(10, -74), new PhoneNumber("+573001234567"),
            new TimeOfDay(22, 0), new TimeOfDay(8, 0)));
    }

    [Fact]
    public void Venue_RequiresSlotInsideOpeningHoursAndHourlyGrid()
    {
        var venue = CreateVenue();
        var valid = new TimeSlot(DateTimeOffset.Parse("2026-11-28T09:00:00-05:00"), DateTimeOffset.Parse("2026-11-28T10:00:00-05:00"));
        Assert.True(venue.IsOpenDuring(valid));
        venue.EnsureFitsHourlyGrid(valid);
        var misaligned = new TimeSlot(valid.Start.AddMinutes(30), valid.End.AddMinutes(30));
        Assert.Throws<BusinessRuleException>(() => venue.EnsureFitsHourlyGrid(misaligned));
    }

    [Fact]
    public void Reservation_RejectsDurationOverFourHoursAndCancellationAfterEnd()
    {
        var start = DateTimeOffset.Parse("2026-11-28T09:00:00-05:00");
        Assert.Throws<BusinessRuleException>(() => new Reservation(Guid.NewGuid(), Guid.NewGuid(),
            new TimeSlot(start, start.AddHours(5)), new Money(100_000, Currency.COP)));
        var reservation = new Reservation(Guid.NewGuid(), Guid.NewGuid(),
            new TimeSlot(start, start.AddHours(1)), new Money(100_000, Currency.COP));
        Assert.Throws<BusinessRuleException>(() => reservation.Cancel(null, start.AddHours(1)));
    }

    [Fact]
    public void Reservation_NoShowCanOnlyBeMarkedAfterStart()
    {
        var start = DateTimeOffset.Parse("2026-11-28T09:00:00-05:00");
        var reservation = new Reservation(Guid.NewGuid(), Guid.NewGuid(), new TimeSlot(start, start.AddHours(1)), new Money(1, Currency.COP));
        reservation.Confirm();
        Assert.Throws<BusinessRuleException>(() => reservation.MarkAsNoShow(start.AddMinutes(-1)));
        reservation.MarkAsNoShow(start);
        Assert.Equal(ReservationStatus.NoShow, reservation.Status);
    }
}


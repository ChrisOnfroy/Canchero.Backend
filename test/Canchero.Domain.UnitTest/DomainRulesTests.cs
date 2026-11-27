using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using Domain.ValueObjects;

namespace Canchero.UnitTest;

public sealed class DomainRulesTests
{
    [Fact]
    public void TimeSlot_RejectsEndBeforeOrEqualToStart()
    {
        var start = DateTimeOffset.Parse("2026-11-27T10:00:00-05:00");
        Assert.Throws<ValidationException>(() => new TimeSlot(start, start));
        Assert.Throws<ValidationException>(() => new TimeSlot(start, start.AddMinutes(-1)));
    }

    [Fact]
    public void TimeSlot_OverlapUsesHalfOpenIntervals()
    {
        var start = DateTimeOffset.Parse("2026-11-27T10:00:00-05:00");
        var first = new TimeSlot(start, start.AddHours(1));
        Assert.False(first.OverlapsWith(new TimeSlot(start.AddHours(1), start.AddHours(2))));
        Assert.True(first.OverlapsWith(new TimeSlot(start.AddMinutes(30), start.AddHours(2))));
    }

    [Fact]
    public void Reservation_ConfirmsThenCompletesOnlyAfterEnd()
    {
        var start = DateTimeOffset.Parse("2026-11-27T10:00:00-05:00");
        var reservation = new Reservation(Guid.NewGuid(), Guid.NewGuid(),
            new TimeSlot(start, start.AddHours(1)), new Money(100_000, Currency.COP));
        reservation.Confirm();
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Throws<BusinessRuleException>(() => reservation.Complete(start.AddMinutes(30)));
        reservation.Complete(start.AddHours(1));
        Assert.Equal(ReservationStatus.Completed, reservation.Status);
    }
}

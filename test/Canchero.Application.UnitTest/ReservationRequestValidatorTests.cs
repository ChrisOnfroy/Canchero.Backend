using Application.DTOs.Reservations.Requests;
using Application.Interfaces.Common;

namespace Canchero.Application.UnitTest;

public sealed class ReservationRequestValidatorTests
{
    private sealed class FixedClock(DateTimeOffset now) : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => now;
    }

    [Fact]
    public void CreateReservation_RequiresActorCourtAndFutureStart()
    {
        var now = DateTimeOffset.Parse("2026-11-27T12:00:00Z");
        var validator = new CreateReservationRequestValidator(new FixedClock(now));
        var invalid = validator.Validate(new CreateReservationRequest(Guid.Empty, Guid.Empty, now, now.AddHours(1)));
        Assert.False(invalid.IsValid);
        Assert.Contains(invalid.Errors, error => error.PropertyName == "ActorUserId");
        Assert.Contains(invalid.Errors, error => error.PropertyName == "CourtId");
        Assert.Contains(invalid.Errors, error => error.PropertyName == "SlotStart");
    }

    [Fact]
    public void CreateReservation_AcceptsFutureStart()
    {
        var now = DateTimeOffset.Parse("2026-11-27T12:00:00Z");
        var validator = new CreateReservationRequestValidator(new FixedClock(now));
        var valid = validator.Validate(new CreateReservationRequest(Guid.NewGuid(), Guid.NewGuid(), now.AddHours(1), now.AddHours(2)));
        Assert.True(valid.IsValid);
    }
}

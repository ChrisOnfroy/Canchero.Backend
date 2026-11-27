using Application.DTOs.Reservations.Requests;
using Application.Errors;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Services.Reservations;
using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using Domain.ValueObjects;
using FluentValidation;
using Moq;

namespace Canchero.Application.UnitTest;

public sealed class CreateReservationServiceTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-11-27T12:00:00Z");

    private static (CreateReservationService Service, Mock<IReservationRepository> Reservations, Guid Actor, Guid CourtId) Build(int pending = 0, bool conflict = false)
    {
        var actor = new User(new PersonName("Test", "Customer"), new Email("customer@example.test"), null, UserRole.Customer);
        var venue = new Venue(Guid.NewGuid(), "Sede", "Demo", new Address("Calle 1", "Barranquilla", "Atlántico", "Colombia"), new Coordinates(10.99, -74.80), new PhoneNumber("+573001234567"), new TimeOfDay(8, 0), new TimeOfDay(22, 0));
        var court = new Court(venue.Id, "Fútbol 5", SportType.Football5, SurfaceType.SyntheticGrass, new Capacity(10), new Money(100_000, Currency.COP), false);
        var users = new Mock<IUserRepository>(); users.Setup(x => x.GetByIdAsync(actor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(actor);
        var courts = new Mock<ICourtRepository>(); courts.Setup(x => x.GetByIdAsync(court.Id, It.IsAny<CancellationToken>())).ReturnsAsync(court);
        var venues = new Mock<IVenueRepository>(); venues.Setup(x => x.GetByIdAsync(venue.Id, It.IsAny<CancellationToken>())).ReturnsAsync(venue);
        var reservations = new Mock<IReservationRepository>();
        reservations.Setup(x => x.CountPendingByUserAsync(actor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pending);
        reservations.Setup(x => x.ExistsActiveConflictAsync(court.Id, It.IsAny<TimeSlot>(), It.IsAny<CancellationToken>())).ReturnsAsync(conflict);
        var unit = new Mock<IUnitOfWork>(); unit.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var clock = new Mock<IDateTimeProvider>(); clock.SetupGet(x => x.UtcNow).Returns(Now);
        var validator = new global::Application.DTOs.Reservations.Requests.CreateReservationRequestValidator(clock.Object);
        return (new CreateReservationService(users.Object, courts.Object, venues.Object, reservations.Object, unit.Object, validator), reservations, actor.Id, court.Id);
    }

    private static CreateReservationRequest Request(Guid actor, Guid court) => new(actor, court, Now.AddDays(1).AddHours(9), Now.AddDays(1).AddHours(10));

    [Fact]
    public async Task CreatesPendingReservationWhenSlotIsAvailable()
    {
        var (service, reservations, actor, court) = Build();
        await service.ExecuteAsync(Request(actor, court));
        reservations.Verify(x => x.AddAsync(It.Is<Reservation>(r => r.Status == ReservationStatus.Pending && r.TotalPrice.Amount == 100_000), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectsWhenUserHasReachedPendingReservationLimit()
    {
        var (service, _, actor, court) = Build(pending: Reservation.MaxPendingPerUser);
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.ExecuteAsync(Request(actor, court)));
    }

    [Fact]
    public async Task RejectsWhenCourtSlotConflicts()
    {
        var (service, _, actor, court) = Build(conflict: true);
        await Assert.ThrowsAsync<ConflictException>(() => service.ExecuteAsync(Request(actor, court)));
    }
}


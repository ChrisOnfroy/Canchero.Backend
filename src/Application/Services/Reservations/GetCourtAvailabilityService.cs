using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Reservations;
using Application.Services.Common;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Services.Reservations;

/// <summary>
/// Calcula franjas de una hora disponibles dentro del horario de apertura de una sede.
/// </summary>
public sealed class GetCourtAvailabilityService(
    IUserRepository users,
    ICourtRepository courts,
    IVenueRepository venues,
    IReservationRepository reservations,
    IValidator<GetCourtAvailabilityRequest> validator) : IGetCourtAvailabilityService
{
    public async Task<GetCourtAvailabilityResponse> ExecuteAsync(
        GetCourtAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);

        var court = await courts.GetByIdAsync(request.CourtId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la cancha solicitada.");
        var venue = await venues.GetByIdAsync(court.VenueId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la sede de la cancha.");
        var dayStart = new DateTimeOffset(
            request.Day.Year,
            request.Day.Month,
            request.Day.Day,
            0,
            0,
            0,
            request.Day.Offset);
        if (!court.IsBookable() || !venue.IsActive)
        {
            return new GetCourtAvailabilityResponse(court.Id, dayStart, Array.Empty<AvailableTimeSlotResponse>());
        }

        var dayEnd = dayStart.AddDays(1);
        var activeReservations = await reservations.GetActiveByCourtAndDayAsync(
            court.Id,
            dayStart,
            dayEnd,
            cancellationToken);

        var opening = new DateTimeOffset(
            request.Day.Year,
            request.Day.Month,
            request.Day.Day,
            venue.OpeningTime.Hour,
            venue.OpeningTime.Minute,
            0,
            request.Day.Offset);
        var closing = new DateTimeOffset(
            request.Day.Year,
            request.Day.Month,
            request.Day.Day,
            venue.ClosingTime.Hour,
            venue.ClosingTime.Minute,
            0,
            request.Day.Offset);
        var availableSlots = new List<AvailableTimeSlotResponse>();

        for (var start = opening; start.AddHours(1) <= closing; start = start.AddHours(1))
        {
            var slot = new TimeSlot(start, start.AddHours(1));
            if (activeReservations.Any(reservation => reservation.ConflictsWith(slot)))
            {
                continue;
            }

            availableSlots.Add(new AvailableTimeSlotResponse(slot.Start, slot.End));
        }

        return new GetCourtAvailabilityResponse(court.Id, dayStart, availableSlots);
    }
}

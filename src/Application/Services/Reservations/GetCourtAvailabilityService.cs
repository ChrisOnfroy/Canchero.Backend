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
        // El día consultado es la fecha de calendario enviada por el cliente; el horario de la sede
        // se interpreta siempre en la zona horaria de la sede, no en la del servidor.
        var date = DateOnly.FromDateTime(request.Day.DateTime);
        var midnight = new TimeOfDay(0, 0);
        var dayStart = venue.AtVenueLocalTime(date, midnight);
        // Para no cambiar el contrato con los clientes, la respuesta conserva el offset enviado en
        // "day": "day" sigue siendo la medianoche de esa fecha en el offset del cliente y las franjas
        // se expresan en ese mismo offset (el instante no cambia, solo su representación).
        var responseDay = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), request.Day.Offset);
        if (!court.IsBookable() || !venue.IsActive)
        {
            return new GetCourtAvailabilityResponse(court.Id, responseDay, Array.Empty<AvailableTimeSlotResponse>());
        }

        var dayEnd = venue.AtVenueLocalTime(date.AddDays(1), midnight);
        var activeReservations = await reservations.GetActiveByCourtAndDayAsync(
            court.Id,
            dayStart,
            dayEnd,
            cancellationToken);

        var opening = venue.AtVenueLocalTime(date, venue.OpeningTime);
        var closing = venue.AtVenueLocalTime(date, venue.ClosingTime);
        var availableSlots = new List<AvailableTimeSlotResponse>();

        for (var start = opening; start.AddHours(1) <= closing; start = start.AddHours(1))
        {
            var slot = new TimeSlot(start, start.AddHours(1));
            if (activeReservations.Any(reservation => reservation.ConflictsWith(slot)))
            {
                continue;
            }

            availableSlots.Add(new AvailableTimeSlotResponse(
                slot.Start.ToOffset(request.Day.Offset),
                slot.End.ToOffset(request.Day.Offset)));
        }

        return new GetCourtAvailabilityResponse(court.Id, responseDay, availableSlots);
    }
}

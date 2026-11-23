using Application.DTOs.Reservations.Responses;
using Domain.Entities;

namespace Application.Mappers;

/// <summary>
/// Convierte reservas del dominio a respuestas de Application.
/// </summary>
public static class ReservationMapper
{
    public static ReservationResponse ToResponse(this Reservation reservation)
        => new(
            reservation.Id,
            reservation.UserId,
            reservation.CourtId,
            reservation.TimeSlot.Start,
            reservation.TimeSlot.End,
            reservation.Status,
            reservation.TotalPrice.Amount,
            reservation.TotalPrice.Currency,
            reservation.CancellationReason);
}

using Application.DTOs.Courts.Responses;
using Domain.Entities;

namespace Application.Mappers;

/// <summary>
/// Convierte canchas del dominio a respuestas de Application.
/// </summary>
public static class CourtMapper
{
    public static CourtResponse ToResponse(this Court court)
        => new(
            court.Id,
            court.VenueId,
            court.Name,
            court.SportType,
            court.SurfaceType,
            court.Capacity.Players,
            court.PricePerHour.Amount,
            court.PricePerHour.Currency,
            court.IsIndoor,
            court.Status);
}

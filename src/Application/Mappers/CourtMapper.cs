using Application.DTOs.Courts.Responses;
using Domain.Entities;
using Domain.ValueObjects;

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

    public static NearbyCourtResponse ToNearbyResponse(
        this Court court,
        Venue venue,
        Distance distance,
        long? slotPrice)
        => new(
            court.Id,
            court.Name,
            venue.Id,
            venue.Name,
            venue.Address.FullAddress,
            distance.Meters,
            distance.Kilometers,
            court.SportType,
            court.SurfaceType,
            court.IsIndoor,
            court.PricePerHour.Amount,
            court.PricePerHour.Currency,
            slotPrice);
}

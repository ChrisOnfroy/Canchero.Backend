using Application.DTOs.Venues.Responses;
using Domain.Entities;

namespace Application.Mappers;

/// <summary>
/// Convierte sedes del dominio a respuestas de Application.
/// </summary>
public static class VenueMapper
{
    public static VenueResponse ToResponse(this Venue venue)
        => new(
            venue.Id,
            venue.OwnerId,
            venue.Name,
            venue.Description,
            venue.Address.Street,
            venue.Address.City,
            venue.Address.Department,
            venue.Address.Country,
            venue.Coordinates.Latitude,
            venue.Coordinates.Longitude,
            venue.PhoneNumber.Value,
            venue.OpeningTime.Hour,
            venue.OpeningTime.Minute,
            venue.ClosingTime.Hour,
            venue.ClosingTime.Minute,
            venue.IsActive);
}

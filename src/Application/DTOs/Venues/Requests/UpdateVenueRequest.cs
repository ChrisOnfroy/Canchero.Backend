namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Solicita la actualización de los datos de una sede.
/// </summary>
public sealed record UpdateVenueRequest(
    Guid ActorUserId,
    Guid VenueId,
    string Name,
    string Description,
    string Street,
    string City,
    string Department,
    string Country,
    double Latitude,
    double Longitude,
    string PhoneNumber,
    int OpeningHour,
    int OpeningMinute,
    int ClosingHour,
    int ClosingMinute,
    string? TimeZoneId = null);

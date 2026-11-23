namespace Application.DTOs.Venues.Responses;

/// <summary>
/// Contiene los datos de una sede.
/// </summary>
public sealed record VenueResponse(
    Guid Id,
    Guid OwnerId,
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
    bool IsActive);

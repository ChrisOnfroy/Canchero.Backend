namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Solicita la creación de una sede para el actor autenticado.
/// </summary>
public sealed record CreateVenueRequest(
    Guid ActorUserId,
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
    int ClosingMinute);

using Domain.Enums;

namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Solicita la creación de una cancha en una sede.
/// </summary>
public sealed record CreateCourtRequest(
    Guid ActorUserId,
    Guid VenueId,
    string Name,
    SportType SportType,
    SurfaceType SurfaceType,
    int Capacity,
    long PricePerHour,
    Currency Currency,
    bool IsIndoor);

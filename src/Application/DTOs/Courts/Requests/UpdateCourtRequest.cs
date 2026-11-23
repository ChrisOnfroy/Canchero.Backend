using Domain.Enums;

namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Solicita la actualización de los datos de una cancha.
/// </summary>
public sealed record UpdateCourtRequest(
    Guid ActorUserId,
    Guid CourtId,
    string Name,
    SurfaceType SurfaceType,
    int Capacity,
    long PricePerHour,
    Currency Currency);

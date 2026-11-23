namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Solicita activar o desactivar una sede.
/// </summary>
public sealed record ChangeVenueStatusRequest(Guid ActorUserId, Guid VenueId, bool IsActive);

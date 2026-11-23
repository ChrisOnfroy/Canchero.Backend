namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Solicita las sedes de un propietario.
/// </summary>
public sealed record ListVenuesByOwnerRequest(Guid ActorUserId, Guid OwnerUserId);

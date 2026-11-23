namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Solicita los datos de una sede.
/// </summary>
public sealed record GetVenueByIdRequest(Guid ActorUserId, Guid VenueId);

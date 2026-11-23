using Application.DTOs.Venues.Requests;
using Application.DTOs.Venues.Responses;

namespace Application.Interfaces.Services.Venues;

/// <summary>
/// Define la actualización de sedes.
/// </summary>
public interface IUpdateVenueService
{
    Task<VenueResponse> ExecuteAsync(UpdateVenueRequest request, CancellationToken cancellationToken = default);
}

using Application.DTOs.Venues.Requests;
using Application.DTOs.Venues.Responses;

namespace Application.Interfaces.Services.Venues;

/// <summary>
/// Define la consulta de sedes por propietario.
/// </summary>
public interface IListVenuesByOwnerService
{
    Task<IReadOnlyList<VenueResponse>> ExecuteAsync(
        ListVenuesByOwnerRequest request,
        CancellationToken cancellationToken = default);
}

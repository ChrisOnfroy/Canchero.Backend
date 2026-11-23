using Application.DTOs.Venues.Requests;
using Application.DTOs.Venues.Responses;

namespace Application.Interfaces.Services.Venues;

/// <summary>
/// Define el cambio de estado de una sede.
/// </summary>
public interface IChangeVenueStatusService
{
    Task<VenueResponse> ExecuteAsync(ChangeVenueStatusRequest request, CancellationToken cancellationToken = default);
}

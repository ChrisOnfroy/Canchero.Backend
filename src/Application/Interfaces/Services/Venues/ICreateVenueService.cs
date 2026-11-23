using Application.DTOs.Venues.Requests;
using Application.DTOs.Venues.Responses;

namespace Application.Interfaces.Services.Venues;

/// <summary>
/// Define la creación de sedes.
/// </summary>
public interface ICreateVenueService
{
    Task<VenueResponse> ExecuteAsync(CreateVenueRequest request, CancellationToken cancellationToken = default);
}

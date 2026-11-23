using Application.DTOs.Venues.Requests;
using Application.DTOs.Venues.Responses;

namespace Application.Interfaces.Services.Venues;

/// <summary>
/// Define la consulta de una sede por identificador.
/// </summary>
public interface IGetVenueByIdService
{
    Task<VenueResponse> ExecuteAsync(GetVenueByIdRequest request, CancellationToken cancellationToken = default);
}

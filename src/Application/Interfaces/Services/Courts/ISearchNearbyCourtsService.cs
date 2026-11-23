using Application.Common;
using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;

namespace Application.Interfaces.Services.Courts;

/// <summary>
/// Define la búsqueda de canchas cercanas.
/// </summary>
public interface ISearchNearbyCourtsService
{
    Task<PagedResult<NearbyCourtResponse>> ExecuteAsync(
        SearchNearbyCourtsRequest request,
        CancellationToken cancellationToken = default);
}

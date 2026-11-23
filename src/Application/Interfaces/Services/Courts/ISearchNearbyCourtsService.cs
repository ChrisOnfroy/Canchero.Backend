using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;

namespace Application.Interfaces.Services.Courts;

/// <summary>
/// Define la búsqueda de canchas cercanas.
/// </summary>
public interface ISearchNearbyCourtsService
{
    Task<SearchNearbyCourtsResponse> ExecuteAsync(
        SearchNearbyCourtsRequest request,
        CancellationToken cancellationToken = default);
}

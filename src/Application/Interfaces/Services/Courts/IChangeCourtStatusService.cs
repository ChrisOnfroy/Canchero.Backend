using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;

namespace Application.Interfaces.Services.Courts;

/// <summary>
/// Define el cambio de estado de una cancha.
/// </summary>
public interface IChangeCourtStatusService
{
    Task<CourtResponse> ExecuteAsync(
        ChangeCourtStatusRequest request,
        CancellationToken cancellationToken = default);
}

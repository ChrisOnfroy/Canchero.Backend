using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;

namespace Application.Interfaces.Services.Courts;

/// <summary>
/// Define la actualización de canchas.
/// </summary>
public interface IUpdateCourtService
{
    Task<CourtResponse> ExecuteAsync(UpdateCourtRequest request, CancellationToken cancellationToken = default);
}

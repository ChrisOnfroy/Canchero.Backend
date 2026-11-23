using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;

namespace Application.Interfaces.Services.Courts;

/// <summary>
/// Define la consulta de una cancha por identificador.
/// </summary>
public interface IGetCourtByIdService
{
    Task<CourtResponse> ExecuteAsync(GetCourtByIdRequest request, CancellationToken cancellationToken = default);
}

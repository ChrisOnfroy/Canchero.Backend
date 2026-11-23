using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;

namespace Application.Interfaces.Services.Courts;

/// <summary>
/// Define la creación de canchas.
/// </summary>
public interface ICreateCourtService
{
    Task<CourtResponse> ExecuteAsync(CreateCourtRequest request, CancellationToken cancellationToken = default);
}

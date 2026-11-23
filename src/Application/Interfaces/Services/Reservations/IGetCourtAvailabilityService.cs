using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;

namespace Application.Interfaces.Services.Reservations;

/// <summary>
/// Define la consulta de franjas disponibles para una cancha.
/// </summary>
public interface IGetCourtAvailabilityService
{
    Task<GetCourtAvailabilityResponse> ExecuteAsync(
        GetCourtAvailabilityRequest request,
        CancellationToken cancellationToken = default);
}

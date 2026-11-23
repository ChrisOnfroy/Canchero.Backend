using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Courts;
using Application.Mappers;
using Application.Services.Common;
using FluentValidation;

namespace Application.Services.Courts;

/// <summary>
/// Obtiene los datos de una cancha.
/// </summary>
public sealed class GetCourtByIdService(
    IUserRepository users,
    ICourtRepository courts,
    IValidator<GetCourtByIdRequest> validator) : IGetCourtByIdService
{
    public async Task<CourtResponse> ExecuteAsync(
        GetCourtByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        var court = await courts.GetByIdAsync(request.CourtId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la cancha solicitada.");
        return court.ToResponse();
    }
}

using Application.DTOs.Venues.Requests;
using Application.DTOs.Venues.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Venues;
using Application.Mappers;
using Application.Services.Common;
using FluentValidation;

namespace Application.Services.Venues;

/// <summary>
/// Obtiene los datos de una sede.
/// </summary>
public sealed class GetVenueByIdService(
    IUserRepository users,
    IVenueRepository venues,
    IValidator<GetVenueByIdRequest> validator) : IGetVenueByIdService
{
    public async Task<VenueResponse> ExecuteAsync(
        GetVenueByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        var venue = await venues.GetByIdAsync(request.VenueId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la sede solicitada.");
        return venue.ToResponse();
    }
}

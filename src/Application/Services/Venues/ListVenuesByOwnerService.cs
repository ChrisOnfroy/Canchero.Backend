using Application.DTOs.Venues.Requests;
using Application.DTOs.Venues.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Venues;
using Application.Mappers;
using Application.Services.Common;
using Domain.Enums;
using FluentValidation;

namespace Application.Services.Venues;

/// <summary>
/// Obtiene las sedes de un propietario autorizado.
/// </summary>
public sealed class ListVenuesByOwnerService(
    IUserRepository users,
    IVenueRepository venues,
    IValidator<ListVenuesByOwnerRequest> validator) : IListVenuesByOwnerService
{
    public async Task<IReadOnlyList<VenueResponse>> ExecuteAsync(
        ListVenuesByOwnerRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        if (actor.Id != request.OwnerUserId && actor.Role != UserRole.Admin)
        {
            throw new ForbiddenException("No tienes permiso para consultar las sedes de este propietario.");
        }

        if (actor.Id == request.OwnerUserId && !actor.CanManageVenues())
        {
            throw new ForbiddenException("No tienes permiso para consultar sedes de propietario.");
        }

        var results = await venues.GetByOwnerAsync(request.OwnerUserId, cancellationToken);
        return results.Select(venue => venue.ToResponse()).ToArray();
    }
}

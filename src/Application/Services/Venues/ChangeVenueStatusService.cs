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
/// Activa o desactiva una sede.
/// </summary>
public sealed class ChangeVenueStatusService(
    IUserRepository users,
    IVenueRepository venues,
    IUnitOfWork unitOfWork,
    IValidator<ChangeVenueStatusRequest> validator) : IChangeVenueStatusService
{
    public async Task<VenueResponse> ExecuteAsync(
        ChangeVenueStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        var venue = await venues.GetByIdAsync(request.VenueId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la sede solicitada.");
        ServiceAuthorization.EnsureVenueManagement(actor, venue);

        if (request.IsActive)
        {
            venue.Activate();
        }
        else
        {
            venue.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return venue.ToResponse();
    }
}

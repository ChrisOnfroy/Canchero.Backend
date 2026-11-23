using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Courts;
using Application.Mappers;
using Application.Services.Common;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Services.Courts;

/// <summary>
/// Actualiza los datos operativos y el precio de una cancha.
/// </summary>
public sealed class UpdateCourtService(
    IUserRepository users,
    IVenueRepository venues,
    ICourtRepository courts,
    IUnitOfWork unitOfWork,
    IValidator<UpdateCourtRequest> validator) : IUpdateCourtService
{
    public async Task<CourtResponse> ExecuteAsync(
        UpdateCourtRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        var court = await courts.GetByIdAsync(request.CourtId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la cancha solicitada.");
        var venue = await venues.GetByIdAsync(court.VenueId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la sede de la cancha.");
        ServiceAuthorization.EnsureVenueManagement(actor, venue);

        court.Rename(request.Name);
        court.ChangeSurface(request.SurfaceType);
        court.ChangeCapacity(new Capacity(request.Capacity));
        court.ChangePrice(new Money(request.PricePerHour, request.Currency));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return court.ToResponse();
    }
}

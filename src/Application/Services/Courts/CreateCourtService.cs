using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Courts;
using Application.Mappers;
using Application.Services.Common;
using Domain.Entities;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Services.Courts;

/// <summary>
/// Crea una cancha en una sede propiedad del actor.
/// </summary>
public sealed class CreateCourtService(
    IUserRepository users,
    IVenueRepository venues,
    ICourtRepository courts,
    IUnitOfWork unitOfWork,
    IValidator<CreateCourtRequest> validator) : ICreateCourtService
{
    public async Task<CourtResponse> ExecuteAsync(
        CreateCourtRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        var venue = await venues.GetByIdAsync(request.VenueId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la sede solicitada.");
        ServiceAuthorization.EnsureVenueOwner(actor, venue);

        var court = new Court(
            venue.Id,
            request.Name,
            request.SportType,
            request.SurfaceType,
            new Capacity(request.Capacity),
            new Money(request.PricePerHour, request.Currency),
            request.IsIndoor);

        await courts.AddAsync(court, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return court.ToResponse();
    }
}

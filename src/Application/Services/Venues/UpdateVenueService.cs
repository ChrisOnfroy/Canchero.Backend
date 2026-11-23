using Application.DTOs.Venues.Requests;
using Application.DTOs.Venues.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Venues;
using Application.Mappers;
using Application.Services.Common;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Services.Venues;

/// <summary>
/// Actualiza los datos y el horario de una sede.
/// </summary>
public sealed class UpdateVenueService(
    IUserRepository users,
    IVenueRepository venues,
    IUnitOfWork unitOfWork,
    IValidator<UpdateVenueRequest> validator) : IUpdateVenueService
{
    public async Task<VenueResponse> ExecuteAsync(
        UpdateVenueRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        var venue = await venues.GetByIdAsync(request.VenueId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la sede solicitada.");
        ServiceAuthorization.EnsureVenueManagement(actor, venue);

        venue.Rename(request.Name);
        venue.ChangeDescription(request.Description);
        venue.Relocate(
            new Address(request.Street, request.City, request.Department, request.Country),
            new Coordinates(request.Latitude, request.Longitude));
        venue.ChangePhone(new PhoneNumber(request.PhoneNumber));
        venue.ChangeSchedule(
            new TimeOfDay(request.OpeningHour, request.OpeningMinute),
            new TimeOfDay(request.ClosingHour, request.ClosingMinute));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return venue.ToResponse();
    }
}

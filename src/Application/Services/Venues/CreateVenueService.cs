using Application.DTOs.Venues.Requests;
using Application.DTOs.Venues.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Venues;
using Application.Mappers;
using Application.Services.Common;
using Domain.Entities;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Services.Venues;

/// <summary>
/// Crea una sede cuyo propietario es el actor autenticado.
/// </summary>
public sealed class CreateVenueService(
    IUserRepository users,
    IVenueRepository venues,
    IUnitOfWork unitOfWork,
    IValidator<CreateVenueRequest> validator) : ICreateVenueService
{
    public async Task<VenueResponse> ExecuteAsync(
        CreateVenueRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        if (!actor.CanManageVenues())
        {
            throw new ForbiddenException("No tienes permiso para crear sedes.");
        }

        var venue = new Venue(
            actor.Id,
            request.Name,
            request.Description,
            new Address(request.Street, request.City, request.Department, request.Country),
            new Coordinates(request.Latitude, request.Longitude),
            new PhoneNumber(request.PhoneNumber),
            new TimeOfDay(request.OpeningHour, request.OpeningMinute),
            new TimeOfDay(request.ClosingHour, request.ClosingMinute),
            request.TimeZoneId);

        await venues.AddAsync(venue, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return venue.ToResponse();
    }
}

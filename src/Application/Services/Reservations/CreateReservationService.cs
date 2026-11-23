using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Reservations;
using Application.Mappers;
using Application.Services.Common;
using Domain.Entities;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Services.Reservations;

/// <summary>
/// Crea una reserva pendiente para el actor autenticado.
/// </summary>
public sealed class CreateReservationService(
    IUserRepository users,
    ICourtRepository courts,
    IVenueRepository venues,
    IReservationRepository reservations,
    IUnitOfWork unitOfWork,
    IValidator<CreateReservationRequest> validator) : ICreateReservationService
{
    public async Task<ReservationResponse> ExecuteAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        if (!actor.CanReserve())
        {
            throw new ForbiddenException("No tienes permiso para crear reservas.");
        }

        var court = await courts.GetByIdAsync(request.CourtId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la cancha solicitada.");
        if (!court.IsBookable())
        {
            throw new ConflictException("La cancha no está disponible para reservas.");
        }

        var venue = await venues.GetByIdAsync(court.VenueId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la sede de la cancha.");
        var slot = new TimeSlot(request.SlotStart, request.SlotEnd);
        if (!venue.IsActive || !venue.IsOpenDuring(slot))
        {
            throw new ConflictException("La sede no está abierta durante el horario solicitado.");
        }

        if (await reservations.ExistsActiveConflictAsync(court.Id, slot, cancellationToken))
        {
            throw new ConflictException("La cancha ya tiene una reserva en el horario solicitado.");
        }

        var reservation = new Reservation(
            actor.Id,
            court.Id,
            slot,
            court.CalculatePrice(slot));
        await reservations.AddAsync(reservation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return reservation.ToResponse();
    }
}

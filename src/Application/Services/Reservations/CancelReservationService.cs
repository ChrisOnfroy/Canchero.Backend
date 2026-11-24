using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;
using Application.Errors;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Reservations;
using Application.Mappers;
using Application.Services.Common;
using FluentValidation;

namespace Application.Services.Reservations;

/// <summary>
/// Cancela una reserva si el actor es su cliente, el propietario de la sede o un administrador.
/// </summary>
public sealed class CancelReservationService(
    IUserRepository users,
    IReservationRepository reservations,
    ICourtRepository courts,
    IVenueRepository venues,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IValidator<CancelReservationRequest> validator) : ICancelReservationService
{
    public async Task<ReservationResponse> ExecuteAsync(
        CancelReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        var reservation = await reservations.GetByIdAsync(request.ReservationId, cancellationToken)
            ?? throw new NotFoundException("No se encontró la reserva solicitada.");

        if (actor.Id != reservation.UserId)
        {
            var court = await courts.GetByIdAsync(reservation.CourtId, cancellationToken)
                ?? throw new NotFoundException("No se encontró la cancha de la reserva.");
            var venue = await venues.GetByIdAsync(court.VenueId, cancellationToken)
                ?? throw new NotFoundException("No se encontró la sede de la cancha.");
            ServiceAuthorization.EnsureVenueManagement(actor, venue);
        }

        reservation.Cancel(request.Reason, dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return reservation.ToResponse();
    }
}

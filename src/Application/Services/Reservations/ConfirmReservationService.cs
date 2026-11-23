using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Reservations;
using Application.Mappers;
using Application.Services.Common;
using FluentValidation;

namespace Application.Services.Reservations;

/// <summary>
/// Confirma una reserva pendiente.
/// </summary>
public sealed class ConfirmReservationService(
    IUserRepository users,
    IReservationRepository reservations,
    ICourtRepository courts,
    IVenueRepository venues,
    IUnitOfWork unitOfWork,
    IValidator<ConfirmReservationRequest> validator) : IConfirmReservationService
{
    public async Task<ReservationResponse> ExecuteAsync(
        ConfirmReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        var reservation = await ServiceAuthorization.GetReservationForVenueManagerAsync(
            actor,
            request.ReservationId,
            reservations,
            courts,
            venues,
            cancellationToken);
        reservation.Confirm();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return reservation.ToResponse();
    }
}

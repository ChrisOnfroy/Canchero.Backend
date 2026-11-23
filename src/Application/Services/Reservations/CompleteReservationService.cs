using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Reservations;
using Application.Mappers;
using Application.Services.Common;
using FluentValidation;

namespace Application.Services.Reservations;

/// <summary>
/// Completa una reserva finalizada.
/// </summary>
public sealed class CompleteReservationService(
    IUserRepository users,
    IReservationRepository reservations,
    ICourtRepository courts,
    IVenueRepository venues,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IValidator<CompleteReservationRequest> validator) : ICompleteReservationService
{
    public async Task<ReservationResponse> ExecuteAsync(
        CompleteReservationRequest request,
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
        reservation.Complete(dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return reservation.ToResponse();
    }
}

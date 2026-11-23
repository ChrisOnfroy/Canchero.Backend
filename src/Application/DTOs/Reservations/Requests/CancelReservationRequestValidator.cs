using FluentValidation;

namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Valida la solicitud de cancelación de una reserva.
/// </summary>
public sealed class CancelReservationRequestValidator : AbstractValidator<CancelReservationRequest>
{
    public CancelReservationRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.ReservationId).NotEmpty();
    }
}

using FluentValidation;

namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Valida la solicitud de confirmación de una reserva.
/// </summary>
public sealed class ConfirmReservationRequestValidator : AbstractValidator<ConfirmReservationRequest>
{
    public ConfirmReservationRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.ReservationId).NotEmpty();
    }
}

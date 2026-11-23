using FluentValidation;

namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Valida la solicitud de finalización de una reserva.
/// </summary>
public sealed class CompleteReservationRequestValidator : AbstractValidator<CompleteReservationRequest>
{
    public CompleteReservationRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.ReservationId).NotEmpty();
    }
}

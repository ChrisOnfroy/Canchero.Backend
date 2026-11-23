using FluentValidation;

namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Valida la solicitud de registro de inasistencia.
/// </summary>
public sealed class MarkReservationAsNoShowRequestValidator : AbstractValidator<MarkReservationAsNoShowRequest>
{
    public MarkReservationAsNoShowRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.ReservationId).NotEmpty();
    }
}

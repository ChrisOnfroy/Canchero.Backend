using Application.Interfaces.Common;
using FluentValidation;

namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Valida la solicitud de creación de una reserva.
/// </summary>
public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.CourtId).NotEmpty();
        RuleFor(request => request.SlotStart)
            .Must(start => start > dateTimeProvider.UtcNow)
            .WithMessage("El horario debe comenzar en el futuro.");
    }
}

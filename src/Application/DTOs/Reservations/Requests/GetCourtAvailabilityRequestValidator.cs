using FluentValidation;

namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Valida la consulta de disponibilidad de una cancha.
/// </summary>
public sealed class GetCourtAvailabilityRequestValidator : AbstractValidator<GetCourtAvailabilityRequest>
{
    public GetCourtAvailabilityRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.CourtId).NotEmpty();
    }
}

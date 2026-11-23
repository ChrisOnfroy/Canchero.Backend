using FluentValidation;

namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Valida la consulta paginada de reservas de una cancha.
/// </summary>
public sealed class ListCourtReservationsRequestValidator : AbstractValidator<ListCourtReservationsRequest>
{
    public ListCourtReservationsRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.CourtId).NotEmpty();
        RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 50);
    }
}

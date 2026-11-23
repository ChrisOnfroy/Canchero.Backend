using FluentValidation;

namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Valida la consulta paginada de reservas de un usuario.
/// </summary>
public sealed class ListUserReservationsRequestValidator : AbstractValidator<ListUserReservationsRequest>
{
    public ListUserReservationsRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 50);
        RuleFor(request => request.Status).IsInEnum();
    }
}

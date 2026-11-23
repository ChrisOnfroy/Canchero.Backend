using FluentValidation;

namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Valida el cambio de estado de una sede.
/// </summary>
public sealed class ChangeVenueStatusRequestValidator : AbstractValidator<ChangeVenueStatusRequest>
{
    public ChangeVenueStatusRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.VenueId).NotEmpty();
    }
}

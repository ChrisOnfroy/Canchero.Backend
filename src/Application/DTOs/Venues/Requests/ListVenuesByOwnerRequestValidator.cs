using FluentValidation;

namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Valida la consulta de sedes por propietario.
/// </summary>
public sealed class ListVenuesByOwnerRequestValidator : AbstractValidator<ListVenuesByOwnerRequest>
{
    public ListVenuesByOwnerRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.OwnerUserId).NotEmpty();
    }
}

using FluentValidation;

namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Valida la solicitud de actualización de una sede.
/// </summary>
public sealed class UpdateVenueRequestValidator : AbstractValidator<UpdateVenueRequest>
{
    public UpdateVenueRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.VenueId).NotEmpty();
    }
}

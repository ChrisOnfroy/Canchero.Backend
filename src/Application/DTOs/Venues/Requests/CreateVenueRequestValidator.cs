using FluentValidation;

namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Valida la solicitud de creación de una sede.
/// </summary>
public sealed class CreateVenueRequestValidator : AbstractValidator<CreateVenueRequest>
{
    public CreateVenueRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
    }
}

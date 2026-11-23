using FluentValidation;

namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Valida la solicitud de creación de una cancha.
/// </summary>
public sealed class CreateCourtRequestValidator : AbstractValidator<CreateCourtRequest>
{
    public CreateCourtRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.VenueId).NotEmpty();
        RuleFor(request => request.SportType).IsInEnum();
        RuleFor(request => request.SurfaceType).IsInEnum();
        RuleFor(request => request.Currency).IsInEnum();
    }
}

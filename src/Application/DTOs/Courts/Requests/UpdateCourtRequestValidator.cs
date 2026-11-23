using FluentValidation;

namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Valida la solicitud de actualización de una cancha.
/// </summary>
public sealed class UpdateCourtRequestValidator : AbstractValidator<UpdateCourtRequest>
{
    public UpdateCourtRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.CourtId).NotEmpty();
        RuleFor(request => request.SurfaceType).IsInEnum();
        RuleFor(request => request.Currency).IsInEnum();
    }
}

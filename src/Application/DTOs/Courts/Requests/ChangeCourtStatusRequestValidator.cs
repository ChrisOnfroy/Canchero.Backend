using FluentValidation;

namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Valida el cambio de estado de una cancha.
/// </summary>
public sealed class ChangeCourtStatusRequestValidator : AbstractValidator<ChangeCourtStatusRequest>
{
    public ChangeCourtStatusRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.CourtId).NotEmpty();
        RuleFor(request => request.Status).IsInEnum();
    }
}

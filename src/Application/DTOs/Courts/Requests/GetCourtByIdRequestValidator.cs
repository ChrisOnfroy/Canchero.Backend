using FluentValidation;

namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Valida la consulta de una cancha.
/// </summary>
public sealed class GetCourtByIdRequestValidator : AbstractValidator<GetCourtByIdRequest>
{
    public GetCourtByIdRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.CourtId).NotEmpty();
    }
}

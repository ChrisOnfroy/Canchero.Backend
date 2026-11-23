using FluentValidation;

namespace Application.DTOs.Users.Requests;

/// <summary>
/// Valida la consulta de un usuario.
/// </summary>
public sealed class GetUserByIdRequestValidator : AbstractValidator<GetUserByIdRequest>
{
    public GetUserByIdRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.UserId).NotEmpty();
    }
}

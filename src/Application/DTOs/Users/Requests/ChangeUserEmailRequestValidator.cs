using FluentValidation;

namespace Application.DTOs.Users.Requests;

public sealed class ChangeUserEmailRequestValidator : AbstractValidator<ChangeUserEmailRequest>
{
    public ChangeUserEmailRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.NewEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);
    }
}

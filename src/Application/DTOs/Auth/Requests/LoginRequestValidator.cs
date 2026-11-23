using FluentValidation;

namespace Application.DTOs.Auth.Requests;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MaximumLength(128);
    }
}

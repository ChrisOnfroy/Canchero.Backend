using Application.DTOs.Auth.Requests;
using Application.DTOs.Auth.Responses;
using Application.Errors;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Auth;
using FluentValidation;

namespace Application.Services.Auth;

public sealed class LoginService(
    IIdentityService identity,
    IUserRepository users,
    ITokenService tokens,
    IValidator<LoginRequest> validator) : ILoginService
{
    private const string InvalidCredentialsMessage = "El email o la contraseña no son válidos.";

    public async Task<AuthResponse> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var userId = await identity.ValidateCredentialsAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (userId is null)
        {
            throw new ForbiddenException(InvalidCredentialsMessage);
        }

        var user = await users.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new ForbiddenException(InvalidCredentialsMessage);
        }

        var accessToken = tokens.GenerateAccessToken(user.Id, user.Email, user.Role);
        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            new UserDetails(user.Id, user.Name.FullName, user.Email.Value, user.Role));
    }
}

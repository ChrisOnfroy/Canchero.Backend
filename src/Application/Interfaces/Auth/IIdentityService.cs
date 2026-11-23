using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Interfaces.Auth;

public interface IIdentityService
{
    Task CreateAccountAsync(
        Guid userId,
        Email email,
        string password,
        UserRole role,
        CancellationToken cancellationToken);

    Task<Guid?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task ChangeEmailAsync(
        Guid userId,
        Email newEmail,
        CancellationToken cancellationToken);
}

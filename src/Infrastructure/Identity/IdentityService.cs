using Application.Interfaces.Auth;
using Domain.Enums;
using Domain.Errors;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity;

/// <summary>
/// Implementa la administración de cuentas mediante ASP.NET Core Identity.
/// </summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> userManager) : IIdentityService
{
    // Se usa para que el login tarde lo mismo exista o no la cuenta (evita deducir qué emails están registrados).
    private static readonly PasswordHasher<ApplicationUser> DummyHasher = new();
    private static readonly ApplicationUser DummyUser = new();
    private static readonly string DummyPasswordHash =
        DummyHasher.HashPassword(DummyUser, Guid.NewGuid().ToString("N"));

    public async Task CreateAccountAsync(
        Guid userId,
        Email email,
        string password,
        UserRole role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (await userManager.FindByEmailAsync(email.Value) is not null
            || await userManager.FindByNameAsync(email.Value) is not null)
        {
            throw new Application.Errors.ConflictException("El email ya está registrado.");
        }

        var applicationUser = new ApplicationUser
        {
            Id = userId,
            Email = email.Value,
            UserName = email.Value,
            LockoutEnabled = true
        };
        var createResult = await userManager.CreateAsync(applicationUser, password);
        if (!createResult.Succeeded)
        {
            ThrowIdentityFailure(createResult);
        }

        var roleResult = await userManager.AddToRoleAsync(applicationUser, role.ToString());
        if (!roleResult.Succeeded)
        {
            ThrowIdentityFailure(roleResult);
        }
    }

    public async Task<Guid?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var applicationUser = await userManager.FindByEmailAsync(email);
        if (applicationUser is null || await userManager.IsLockedOutAsync(applicationUser))
        {
            DummyHasher.VerifyHashedPassword(DummyUser, DummyPasswordHash, password);
            return null;
        }

        if (!await userManager.CheckPasswordAsync(applicationUser, password))
        {
            await userManager.AccessFailedAsync(applicationUser);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(applicationUser);
        return applicationUser.Id;
    }

    public async Task ChangeEmailAsync(
        Guid userId,
        Email newEmail,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var applicationUser = await userManager.Users
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken)
            ?? throw new Application.Errors.NotFoundException("No se encontró la cuenta de Identity.");

        var existingEmail = await userManager.FindByEmailAsync(newEmail.Value);
        if (existingEmail is not null && existingEmail.Id != userId)
        {
            throw new Application.Errors.ConflictException("El email ya está registrado.");
        }

        var existingUserName = await userManager.FindByNameAsync(newEmail.Value);
        if (existingUserName is not null && existingUserName.Id != userId)
        {
            throw new Application.Errors.ConflictException("El email ya está registrado.");
        }

        var emailResult = await userManager.SetEmailAsync(applicationUser, newEmail.Value);
        if (!emailResult.Succeeded)
        {
            ThrowIdentityFailure(emailResult);
        }

        var userNameResult = await userManager.SetUserNameAsync(applicationUser, newEmail.Value);
        if (!userNameResult.Succeeded)
        {
            ThrowIdentityFailure(userNameResult);
        }
    }

    private static void ThrowIdentityFailure(IdentityResult result)
    {
        var errors = result.Errors.ToArray();
        if (errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName"))
        {
            throw new Application.Errors.ConflictException("El email ya está registrado.");
        }

        if (errors.Any(error => error.Code.StartsWith("Password", StringComparison.Ordinal)))
        {
            var reasons = errors
                .Where(error => error.Code.StartsWith("Password", StringComparison.Ordinal))
                .Select(error => error.Code switch
                {
                    "PasswordTooShort" => "La contraseña debe tener al menos 8 caracteres.",
                    "PasswordRequiresUpper" => "La contraseña debe incluir una letra mayúscula.",
                    "PasswordRequiresLower" => "La contraseña debe incluir una letra minúscula.",
                    "PasswordRequiresDigit" => "La contraseña debe incluir un número.",
                    _ => "La contraseña no cumple con la política de seguridad."
                });
            throw new ValidationException(string.Join(" ", reasons));
        }

        throw new InvalidOperationException(
            $"Identity no pudo completar la operación: {string.Join("; ", errors.Select(error => error.Description))}");
    }
}

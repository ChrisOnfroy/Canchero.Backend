using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Identity;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Context;

/// <summary>
/// Crea idempotentemente los roles requeridos y el administrador configurado.
/// </summary>
public sealed class DatabaseSeeder(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    IUnitOfWork unitOfWork,
    IOptions<AdminSeedSettings> settings)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var role in Enum.GetNames<UserRole>())
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            EnsureSucceeded(result, $"No fue posible crear el rol {role}.");
        }

        var admin = settings.Value;
        if (string.IsNullOrWhiteSpace(admin.Email)
            || string.IsNullOrWhiteSpace(admin.Password)
            || string.IsNullOrWhiteSpace(admin.FirstName)
            || string.IsNullOrWhiteSpace(admin.LastName))
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var account = await userManager.FindByEmailAsync(admin.Email);
        User profile;
        if (account is null)
        {
            var email = new Email(admin.Email);
            profile = new User(
                new PersonName(admin.FirstName, admin.LastName),
                email,
                null,
                UserRole.Admin);
            await context.UserProfiles.AddAsync(profile, cancellationToken);

            account = new ApplicationUser
            {
                Id = profile.Id,
                Email = email.Value,
                UserName = email.Value,
                LockoutEnabled = true
            };
            var createResult = await userManager.CreateAsync(account, admin.Password);
            EnsureSucceeded(createResult, "No fue posible crear el usuario administrador.");
        }
        else
        {
            profile = await context.UserProfiles.FirstOrDefaultAsync(
                    user => user.Id == account.Id,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "La cuenta de Identity existe sin un perfil de usuario asociado. Repara la relación antes de sembrar.");
            if (profile.Role != UserRole.Admin)
            {
                throw new InvalidOperationException(
                    "La cuenta configurada como administrador tiene un perfil con un rol diferente.");
            }
        }

        if (!await userManager.IsInRoleAsync(account, UserRole.Admin.ToString()))
        {
            var roleResult = await userManager.AddToRoleAsync(account, UserRole.Admin.ToString());
            EnsureSucceeded(roleResult, "No fue posible asignar el rol de administrador.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"{message} {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}

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
    private const string DemoPassword = "CancheroDemo123";
    private const string AdminPassword = "CancheroAdmin123";

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
            var createResult = await userManager.CreateAsync(account, AdminPassword);
            EnsureSucceeded(createResult, "No fue posible crear el usuario administrador.");
        }
        else
        {
            profile = await context.UserProfiles.FirstOrDefaultAsync(
                    user => user.Id == account.Id,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "La cuenta de Identity existe sin un perfil de usuario asociado. Repara la relaciÃ³n antes de sembrar.");
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

        await SeedDemoDataAsync(cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task SeedDemoDataAsync(CancellationToken cancellationToken)
    {
        // Datos de demostraciÃ³n editables: las ubicaciones son aproximadas y deben verificarse.
        var owners = new[]
        {
            ("owner1@canchero.test", "Carlos", "Cancha"),
            ("owner2@canchero.test", "MarÃ­a", "Deportes"),
            ("owner3@canchero.test", "AndrÃ©s", "FÃºtbol"),
            ("owner4@canchero.test", "Laura", "Arena")
        };
        var ownerIds = new List<Guid>();
        foreach (var owner in owners)
            ownerIds.Add(await EnsureDemoUserAsync(owner.Item1, owner.Item2, owner.Item3, UserRole.VenueOwner, cancellationToken));

        var customers = new[]
        {
            ("customer1@canchero.test", "Ana", "Prueba"),
            ("customer2@canchero.test", "Juan", "Prueba"),
            ("customer3@canchero.test", "SofÃ­a", "Prueba")
        };
        foreach (var customer in customers)
            await EnsureDemoUserAsync(customer.Item1, customer.Item2, customer.Item3, UserRole.Customer, cancellationToken);

        if (await context.Venues.AnyAsync(cancellationToken)) return;
        var venues = new[]
        {
            new Venue(ownerIds[0], "Complejo Deportivo Norte (demo)", "Sede de prueba; verificar direcciÃ³n y coordenadas.", new Address("Calle 98 # 52-115", "Barranquilla", "AtlÃ¡ntico", "Colombia"), new Coordinates(11.0125, -74.8280), new PhoneNumber("+573001000001"), new TimeOfDay(6,0), new TimeOfDay(23,0)),
            new Venue(ownerIds[1], "Canchas Villa Santos (demo)", "Sede de prueba; verificar direcciÃ³n y coordenadas.", new Address("Carrera 51B #  à²† 90", "Barranquilla", "AtlÃ¡ntico", "Colombia"), new Coordinates(11.0100, -74.8170), new PhoneNumber("+573001000002"), new TimeOfDay(6,0), new TimeOfDay(23,0)),
            new Venue(ownerIds[2], "Complejo La Castellana (demo)", "Sede de prueba; verificar direcciÃ³n y coordenadas.", new Address("Calle 84 # 42F", "Barranquilla", "AtlÃ¡ntico", "Colombia"), new Coordinates(10.9890, -74.8180), new PhoneNumber("+573001000003"), new TimeOfDay(6,0), new TimeOfDay(23,0)),
            new Venue(ownerIds[3], "Centro Deportivo Recreo (demo)", "Sede de prueba; verificar direcciÃ³n y coordenadas.", new Address("Carrera 38 # 61", "Barranquilla", "AtlÃ¡ntico", "Colombia"), new Coordinates(10.9740, -74.8010), new PhoneNumber("+573001000004"), new TimeOfDay(6,0), new TimeOfDay(23,0))
        };
        await context.Venues.AddRangeAsync(venues, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        var courts = new[]
        {
            new Court(venues[0].Id, "FÃºtbol 5", SportType.Football5, SurfaceType.SyntheticGrass, new Capacity(10), new Money(120000, Currency.COP), false),
            new Court(venues[0].Id, "FÃºtbol 7", SportType.Football7, SurfaceType.SyntheticGrass, new Capacity(14), new Money(180000, Currency.COP), false),
            new Court(venues[1].Id, "FÃºtbol 5", SportType.Football5, SurfaceType.SyntheticGrass, new Capacity(10), new Money(130000, Currency.COP), false),
            new Court(venues[1].Id, "PÃ¡del", SportType.Padel, SurfaceType.Concrete, new Capacity(4), new Money(90000, Currency.COP), true),
            new Court(venues[2].Id, "FÃºtbol 11", SportType.Football11, SurfaceType.NaturalGrass, new Capacity(22), new Money(300000, Currency.COP), false),
            new Court(venues[2].Id, "Baloncesto", SportType.Basketball, SurfaceType.Concrete, new Capacity(10), new Money(80000, Currency.COP), false),
            new Court(venues[3].Id, "Voleibol", SportType.Volleyball, SurfaceType.Sand, new Capacity(12), new Money(70000, Currency.COP), false),
            new Court(venues[3].Id, "Tenis", SportType.Tennis, SurfaceType.Clay, new Capacity(4), new Money(60000, Currency.COP), false)
        };
        await context.Courts.AddRangeAsync(courts, cancellationToken);
    }

    private async Task<Guid> EnsureDemoUserAsync(string emailAddress, string firstName, string lastName, UserRole role, CancellationToken cancellationToken)
    {
        var account = await userManager.FindByEmailAsync(emailAddress);
        if (account is null)
        {
            var email = new Email(emailAddress);
            var profile = new User(new PersonName(firstName, lastName), email, null, role);
            await context.UserProfiles.AddAsync(profile, cancellationToken);
            account = new ApplicationUser { Id = profile.Id, Email = email.Value, UserName = email.Value, LockoutEnabled = true };
            EnsureSucceeded(await userManager.CreateAsync(account, DemoPassword), $"No fue posible crear {emailAddress}.");
        }
        if (!await userManager.IsInRoleAsync(account, role.ToString()))
            EnsureSucceeded(await userManager.AddToRoleAsync(account, role.ToString()), $"No fue posible asignar el rol {role}.");
        return account.Id;
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


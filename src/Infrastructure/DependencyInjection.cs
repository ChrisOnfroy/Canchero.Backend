using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Application.Interfaces.Geocoding;
using Application.Interfaces.Repositories;
using Infrastructure.Context;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Registra los adaptadores de persistencia, identidad y proveedores externos.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Falta ConnectionStrings:DefaultConnection. Configura la variable "
                + "ConnectionStrings__DefaultConnection o, para desarrollo local, inicia con "
                + "ASPNETCORE_ENVIRONMENT=Development y configura appsettings.Development.json.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<ICourtRepository, CourtRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<DatabaseSeeder>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddMemoryCache();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .Validate(settings =>
                    !string.IsNullOrWhiteSpace(settings.Issuer)
                    && !string.IsNullOrWhiteSpace(settings.Audience)
                    && settings.SecretKey.Length >= 32
                    && settings.ExpirationMinutes > 0,
                "La configuración Jwt requiere Issuer, Audience, SecretKey (mínimo 32 caracteres) y ExpirationMinutes válido.")
            .ValidateOnStart();

        services.AddOptions<GeocodingSettings>()
            .Bind(configuration.GetSection("Geocoding"))
            .Validate(settings =>
                    Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var baseUri)
                    && (baseUri.Scheme == Uri.UriSchemeHttps || baseUri.Scheme == Uri.UriSchemeHttp)
                    && !string.IsNullOrWhiteSpace(settings.UserAgent)
                    && !string.IsNullOrWhiteSpace(settings.CountryCodes)
                    && settings.CacheMinutes > 0
                    && settings.TimeoutSeconds > 0,
                "La configuración Geocoding requiere BaseUrl HTTP(S), UserAgent, CountryCodes y duraciones positivas.")
            .ValidateOnStart();

        services.AddOptions<AdminSeedSettings>()
            .Bind(configuration.GetSection("Admin"))
            .Validate(settings =>
            {
                var values = new[]
                {
                    settings.Email,
                    settings.Password,
                    settings.FirstName,
                    settings.LastName
                };
                return values.All(string.IsNullOrWhiteSpace)
                    || values.All(value => !string.IsNullOrWhiteSpace(value));
            }, "La sección Admin debe estar vacía o incluir Email, Password, FirstName y LastName.")
            .ValidateOnStart();

        services.AddHttpClient<NominatimGeocodingService>((provider, client) =>
        {
            var settings = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<GeocodingSettings>>().Value;
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });
        services.AddScoped<IGeocodingService>(provider =>
            provider.GetRequiredService<NominatimGeocodingService>());

        return services;
    }
}

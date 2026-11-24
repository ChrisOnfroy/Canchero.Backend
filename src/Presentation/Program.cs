using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Application;
using Infrastructure;
using Infrastructure.Context;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Presentation.Errors;
using Presentation.Workers;
using Swashbuckle.AspNetCore.SwaggerGen;

var configuredEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // Al ejecutar el binario directamente (por ejemplo, desde Rider), carga por defecto
    // la configuración local. Los despliegues deben declarar explícitamente su entorno.
    EnvironmentName = string.IsNullOrWhiteSpace(configuredEnvironment)
        ? Environments.Development
        : configuredEnvironment
});

if (builder.Environment.IsDevelopment()
    && string.IsNullOrWhiteSpace(builder.Configuration["Jwt:SecretKey"]))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Jwt:SecretKey"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48))
    });
}

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHostedService<PendingReservationExpirationWorker>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredOrigins", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader();
        }
    });
});

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Falta la sección Jwt de configuración.");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateLifetime = true,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Canchero API",
        Version = "v1",
        Description = """
            API del sistema Canchero para la búsqueda y reserva de canchas deportivas.

            Flujo de autenticación: regístrate, inicia sesión, copia el token JWT y pulsa **Authorize**.
            Pega solamente el token, sin la palabra `Bearer`.
            """
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega solo el token JWT obtenido al iniciar sesión, sin la palabra Bearer."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }] = Array.Empty<string>()
    });
    options.OperationFilter<AllowAnonymousOperationFilter>();
    options.OrderActionsBy(description =>
    {
        var routeValues = description.ActionDescriptor.RouteValues;
        var controller = routeValues.TryGetValue("controller", out var value)
            ? value
            : string.Empty;
        return $"{controller}_{description.HttpMethod}";
    });

    IncludeXmlComments(options, typeof(Program).Assembly);
    IncludeXmlComments(options, Assembly.Load("Application"));
});

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:Migrate"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await scope.ServiceProvider
        .GetRequiredService<DatabaseSeeder>()
        .SeedAsync();
}

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    if (!app.Environment.IsDevelopment())
    {
        app.Logger.LogWarning(
            "Swagger está habilitado fuera de Development; la documentación de la API está expuesta.");
    }

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Canchero API v1");
        options.RoutePrefix = "swagger";
        options.EnablePersistAuthorization();
        options.DisplayRequestDuration();
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("ConfiguredOrigins");
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithTags("Health")
    .WithSummary("Comprueba el estado de la API.")
    .Produces(StatusCodes.Status200OK)
    .AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription()
        .AllowAnonymous();
}

app.Run();

static void IncludeXmlComments(SwaggerGenOptions options, Assembly assembly)
{
    var documentationFile = Path.Combine(
        AppContext.BaseDirectory,
        $"{assembly.GetName().Name}.xml");
    if (File.Exists(documentationFile))
    {
        options.IncludeXmlComments(documentationFile, includeControllerXmlComments: true);
    }
}

/// <summary>
/// Quita el requisito global de autenticación de las acciones anónimas en Swagger.
/// </summary>
public sealed class AllowAnonymousOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any())
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
            };
        }
    }
}

using Application.Interfaces.Common;
using FluentValidation;

namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Valida los filtros y la paginación de la búsqueda de canchas.
/// </summary>
public sealed class SearchNearbyCourtsRequestValidator : AbstractValidator<SearchNearbyCourtsRequest>
{
    public SearchNearbyCourtsRequestValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request)
            .Must(request => request.LocationText is not null
                ? request.Latitude is null && request.Longitude is null
                : request.Latitude.HasValue && request.Longitude.HasValue)
            .WithMessage("Indica una ubicación en texto o las coordenadas completas, pero no ambas.");
        RuleFor(request => request.LocationText)
            .Must(location => location is not null
                && location.Trim().Length is >= 3 and <= 200)
            .When(request => request.LocationText is not null)
            .WithMessage("La ubicación debe tener entre 3 y 200 caracteres.");
        RuleFor(request => request.RadiusKm).InclusiveBetween(0.01, 100);
        RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 50);
        RuleFor(request => request.SportType).IsInEnum();
        RuleFor(request => request.SurfaceType).IsInEnum();
        RuleFor(request => request.PriceCurrency).IsInEnum();
        RuleFor(request => request.SlotEnd)
            .NotNull()
            .When(request => request.SlotStart.HasValue);
        RuleFor(request => request.SlotStart)
            .NotNull()
            .When(request => request.SlotEnd.HasValue);
        RuleFor(request => request.SlotStart)
            .Must(start => start > dateTimeProvider.UtcNow)
            .When(request => request.SlotStart.HasValue)
            .WithMessage("El horario debe comenzar en el futuro.");
    }
}

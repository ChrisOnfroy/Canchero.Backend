using FluentValidation;

namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Valida la solicitud de actualización de una sede.
/// </summary>
public sealed class UpdateVenueRequestValidator : AbstractValidator<UpdateVenueRequest>
{
    public UpdateVenueRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.VenueId).NotEmpty();
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Must(name => name is null || name.Trim().Length <= 100)
            .WithMessage("El nombre no puede exceder 100 caracteres.");
        RuleFor(request => request.Description)
            .NotNull().WithMessage("La descripción no puede ser nula.")
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");
        RuleFor(request => request.Street).NotEmpty().WithMessage("La calle es obligatoria.");
        RuleFor(request => request.City).NotEmpty().WithMessage("La ciudad es obligatoria.");
        RuleFor(request => request.Department).NotEmpty().WithMessage("El departamento es obligatorio.");
        RuleFor(request => request.Country).NotEmpty().WithMessage("El país es obligatorio.");
        RuleFor(request => request.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("La latitud debe estar entre -90 y 90.");
        RuleFor(request => request.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("La longitud debe estar entre -180 y 180.");
        RuleFor(request => request.PhoneNumber).NotEmpty().WithMessage("El teléfono es obligatorio.");
        RuleFor(request => request.OpeningHour)
            .InclusiveBetween(0, 23).WithMessage("La hora de apertura debe estar entre 0 y 23.");
        RuleFor(request => request.OpeningMinute)
            .InclusiveBetween(0, 59).WithMessage("Los minutos de apertura deben estar entre 0 y 59.");
        RuleFor(request => request.ClosingHour)
            .InclusiveBetween(0, 23).WithMessage("La hora de cierre debe estar entre 0 y 23.");
        RuleFor(request => request.ClosingMinute)
            .InclusiveBetween(0, 59).WithMessage("Los minutos de cierre deben estar entre 0 y 59.");
        RuleFor(request => request.TimeZoneId)
            .MaximumLength(64).WithMessage("La zona horaria no puede exceder 64 caracteres.");
    }
}

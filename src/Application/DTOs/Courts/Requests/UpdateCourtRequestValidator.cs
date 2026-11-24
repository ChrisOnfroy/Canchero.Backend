using FluentValidation;

namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Valida la solicitud de actualización de una cancha.
/// </summary>
public sealed class UpdateCourtRequestValidator : AbstractValidator<UpdateCourtRequest>
{
    public UpdateCourtRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.CourtId).NotEmpty();
        RuleFor(request => request.SurfaceType).IsInEnum();
        RuleFor(request => request.Currency).IsInEnum();
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Must(name => name is null || name.Trim().Length <= 80)
            .WithMessage("El nombre no puede exceder 80 caracteres.");
        RuleFor(request => request.Capacity)
            .InclusiveBetween(1, 100).WithMessage("La capacidad debe estar entre 1 y 100 jugadores.");
        RuleFor(request => request.PricePerHour)
            .GreaterThan(0).WithMessage("El precio por hora debe ser mayor que cero.");
    }
}

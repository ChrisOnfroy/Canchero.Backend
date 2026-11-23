using FluentValidation;

namespace Application.DTOs.Venues.Requests;

/// <summary>
/// Valida la consulta de una sede.
/// </summary>
public sealed class GetVenueByIdRequestValidator : AbstractValidator<GetVenueByIdRequest>
{
    public GetVenueByIdRequestValidator()
    {
        RuleFor(request => request.ActorUserId).NotEmpty();
        RuleFor(request => request.VenueId).NotEmpty();
    }
}

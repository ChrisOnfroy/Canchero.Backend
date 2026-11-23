using Application.Common;
using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Reservations;
using Application.Mappers;
using Application.Services.Common;
using Domain.Enums;
using FluentValidation;

namespace Application.Services.Reservations;

/// <summary>
/// Obtiene una página de reservas del usuario solicitado.
/// </summary>
public sealed class ListUserReservationsService(
    IUserRepository users,
    IReservationRepository reservations,
    IValidator<ListUserReservationsRequest> validator) : IListUserReservationsService
{
    public async Task<PagedResult<ReservationResponse>> ExecuteAsync(
        ListUserReservationsRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        if (actor.Id != request.UserId && actor.Role != UserRole.Admin)
        {
            throw new ForbiddenException("No tienes permiso para consultar estas reservas.");
        }

        var result = await reservations.GetByUserAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            request.Status,
            cancellationToken);
        return new PagedResult<ReservationResponse>(
            result.Items.Select(reservation => reservation.ToResponse()).ToArray(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }
}

using Application.DTOs.Users.Requests;
using Application.DTOs.Users.Responses;
using Application.Errors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Users;
using Application.Mappers;
using Application.Services.Common;
using Domain.Enums;
using FluentValidation;

namespace Application.Services.Users;

/// <summary>
/// Obtiene los datos de un usuario, respetando la autorización del actor.
/// </summary>
public sealed class GetUserByIdService(
    IUserRepository users,
    IValidator<GetUserByIdRequest> validator) : IGetUserByIdService
{
    public async Task<UserResponse> ExecuteAsync(
        GetUserByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var actor = await ServiceAuthorization.GetActiveActorAsync(users, request.ActorUserId, cancellationToken);
        if (actor.Id != request.UserId && actor.Role != UserRole.Admin)
        {
            throw new ForbiddenException("No tienes permiso para consultar este usuario.");
        }

        var user = await users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("No se encontró el usuario solicitado.");
        return user.ToResponse();
    }
}

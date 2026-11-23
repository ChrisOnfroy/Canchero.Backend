using Application.DTOs.Users.Requests;
using Application.Errors;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Users;
using Domain.Enums;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Services.Users;

public sealed class ChangeUserEmailService(
    IUserRepository users,
    IIdentityService identity,
    IUnitOfWork unitOfWork,
    IValidator<ChangeUserEmailRequest> validator) : IChangeUserEmailService
{
    public async Task ExecuteAsync(
        ChangeUserEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var actor = await users.GetByIdAsync(request.ActorUserId, cancellationToken);
        if (actor is null || !actor.IsActive)
        {
            throw new ForbiddenException("No tienes permiso para cambiar este email.");
        }

        if (request.ActorUserId != request.UserId && actor.Role != UserRole.Admin)
        {
            throw new ForbiddenException("No tienes permiso para cambiar este email.");
        }

        var user = request.ActorUserId == request.UserId
            ? actor
            : await users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new ForbiddenException("No tienes permiso para cambiar este email.");
        }

        var newEmail = new Email(request.NewEmail);
        if (await users.GetByEmailAsync(newEmail, cancellationToken) is { } existingUser
            && existingUser.Id != user.Id)
        {
            throw new ConflictException("Ya existe un usuario con este email.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await identity.ChangeEmailAsync(user.Id, newEmail, cancellationToken);
            user.ChangeEmail(newEmail);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}

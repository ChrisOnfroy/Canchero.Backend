using Application.DTOs.Auth.Requests;
using Application.Errors;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Auth;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Services.Auth;

public sealed class RegisterUserService(
    IUserRepository users,
    IIdentityService identity,
    IUnitOfWork unitOfWork,
    IValidator<RegisterUserRequest> validator) : IRegisterUserService
{
    public async Task ExecuteAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Role is not UserRole.Customer and not UserRole.VenueOwner)
        {
            throw new ForbiddenException("No está permitido registrar este rol.");
        }

        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var email = new Email(request.Email);
        if (await users.GetByEmailAsync(email, cancellationToken) is not null)
        {
            throw new ConflictException("Ya existe un usuario con este email.");
        }

        var user = new User(
            new PersonName(request.FirstName, request.LastName),
            email,
            request.PhoneNumber is null ? null : new PhoneNumber(request.PhoneNumber),
            request.Role);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await users.AddAsync(user, cancellationToken);
            await identity.CreateAccountAsync(user.Id, email, request.Password, request.Role, cancellationToken);
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

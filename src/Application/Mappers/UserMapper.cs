using Application.DTOs.Users.Responses;
using Domain.Entities;

namespace Application.Mappers;

/// <summary>
/// Convierte usuarios del dominio a respuestas de Application.
/// </summary>
public static class UserMapper
{
    public static UserResponse ToResponse(this User user)
        => new(
            user.Id,
            user.Name.FirstName,
            user.Name.LastName,
            user.Email.Value,
            user.PhoneNumber?.Value,
            user.Role,
            user.IsActive);
}

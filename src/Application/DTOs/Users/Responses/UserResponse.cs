using Domain.Enums;

namespace Application.DTOs.Users.Responses;

/// <summary>
/// Contiene los datos públicos de un usuario.
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    UserRole Role,
    bool IsActive);

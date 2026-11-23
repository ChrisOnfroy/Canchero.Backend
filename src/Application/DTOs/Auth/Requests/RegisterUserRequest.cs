using Domain.Enums;

namespace Application.DTOs.Auth.Requests;

public sealed record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Password,
    UserRole Role);

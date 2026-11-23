using Domain.Enums;

namespace Application.DTOs.Auth.Responses;

public sealed record AuthResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    UserDetails User);

public sealed record UserDetails(Guid Id, string Name, string Email, UserRole Role);

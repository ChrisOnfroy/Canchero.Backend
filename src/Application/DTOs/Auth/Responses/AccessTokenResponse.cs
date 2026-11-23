namespace Application.DTOs.Auth.Responses;

public sealed record AccessTokenResponse(string Token, DateTimeOffset ExpiresAt);

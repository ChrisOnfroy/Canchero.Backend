using Application.DTOs.Auth.Responses;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Interfaces.Auth;

public interface ITokenService
{
    AccessTokenResponse GenerateAccessToken(Guid userId, Email email, UserRole role);
}

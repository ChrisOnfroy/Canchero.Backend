using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs.Auth.Responses;
using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

/// <summary>
/// Genera tokens JWT firmados con HMAC SHA-256.
/// </summary>
public sealed class JwtTokenService(
    IOptions<JwtSettings> options,
    IDateTimeProvider dateTimeProvider) : ITokenService
{
    private readonly JwtSettings _settings = options.Value;

    public AccessTokenResponse GenerateAccessToken(Guid userId, Email email, UserRole role)
    {
        var now = dateTimeProvider.UtcNow;
        var expiresAt = now.AddMinutes(_settings.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email.Value),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _settings.Issuer,
            _settings.Audience,
            claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);
        return new AccessTokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

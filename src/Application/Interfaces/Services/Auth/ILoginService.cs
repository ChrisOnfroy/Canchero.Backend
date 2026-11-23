using Application.DTOs.Auth.Requests;
using Application.DTOs.Auth.Responses;

namespace Application.Interfaces.Services.Auth;

public interface ILoginService
{
    Task<AuthResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

using Application.DTOs.Auth.Requests;

namespace Application.Interfaces.Services.Auth;

public interface IRegisterUserService
{
    Task ExecuteAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
}

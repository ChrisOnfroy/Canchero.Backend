using Application.DTOs.Users.Requests;

namespace Application.Interfaces.Services.Users;

public interface IChangeUserEmailService
{
    Task ExecuteAsync(ChangeUserEmailRequest request, CancellationToken cancellationToken = default);
}

using Application.DTOs.Users.Requests;
using Application.DTOs.Users.Responses;

namespace Application.Interfaces.Services.Users;

/// <summary>
/// Define la consulta de un usuario por identificador.
/// </summary>
public interface IGetUserByIdService
{
    Task<UserResponse> ExecuteAsync(GetUserByIdRequest request, CancellationToken cancellationToken = default);
}

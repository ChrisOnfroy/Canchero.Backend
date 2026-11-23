using System.Security.Claims;
using Application.DTOs.Users.Requests;
using Application.DTOs.Users.Responses;
using Application.Interfaces.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(IGetUserByIdService getUser, IChangeUserEmailService changeEmail) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Get(Guid id, CancellationToken ct) =>
        Ok(await getUser.ExecuteAsync(new GetUserByIdRequest(ActorId(), id), ct));

    [HttpPut("{id:guid}/email")]
    public async Task<IActionResult> ChangeEmail(Guid id, ChangeEmailBody body, CancellationToken ct)
    {
        await changeEmail.ExecuteAsync(new ChangeUserEmailRequest(ActorId(), id, body.NewEmail), ct);
        return NoContent();
    }

    private Guid ActorId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public sealed record ChangeEmailBody(string NewEmail);
}

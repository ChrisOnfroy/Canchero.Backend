using Application.DTOs.Auth.Requests;

namespace Canchero.Application.UnitTest;

public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void RejectsMissingEmailAndPassword()
    {
        var result = _validator.Validate(new LoginRequest("", ""));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginRequest.Email));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginRequest.Password));
    }
}


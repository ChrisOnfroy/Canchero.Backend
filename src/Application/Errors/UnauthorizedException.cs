namespace Application.Errors;

/// <summary>
/// Indica que la identidad del solicitante no pudo comprobarse (por ejemplo, credenciales inválidas).
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}

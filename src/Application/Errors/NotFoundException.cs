namespace Application.Errors;

/// <summary>
/// Indica que no se encontró el recurso solicitado.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}

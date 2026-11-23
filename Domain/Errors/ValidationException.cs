namespace Domain.Errors;

/// <summary>
/// Indica que un dato no cumple una invariante de validación.
/// </summary>
public sealed class ValidationException : DomainException
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

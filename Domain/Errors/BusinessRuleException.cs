namespace Domain.Errors;

/// <summary>
/// Indica que se intentó vulnerar una regla de negocio.
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message)
    {
    }

    public BusinessRuleException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

namespace Application.Interfaces.Common;

/// <summary>
/// Proporciona la hora UTC actual.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}

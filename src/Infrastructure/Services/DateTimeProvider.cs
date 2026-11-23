using Application.Interfaces.Common;

namespace Infrastructure.Services;

/// <summary>
/// Proporciona la hora UTC del sistema.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

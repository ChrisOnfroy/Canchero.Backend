using System.Text.RegularExpressions;
using Domain.Errors;

namespace Domain.Commons;

/// <summary>
/// Contiene validaciones básicas de datos.
/// </summary>
public static class Guard
{
    public static void AgainstNull(object? value, string parameterName)
    {
        if (value is null)
        {
            throw new ValidationException($"El parámetro '{parameterName}' no puede ser nulo.");
        }
    }

    public static void AgainstEmptyString(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"El parámetro '{parameterName}' no puede estar vacío.");
        }
    }

    public static void AgainstMaxLength(string? value, int maxLength, string parameterName)
    {
        AgainstNull(value, parameterName);
        if (value!.Length > maxLength)
        {
            throw new ValidationException($"El parámetro '{parameterName}' no puede exceder {maxLength} caracteres.");
        }
    }

    public static void AgainstMinLength(string? value, int minLength, string parameterName)
    {
        AgainstNull(value, parameterName);
        if (value!.Length < minLength)
        {
            throw new ValidationException($"El parámetro '{parameterName}' debe tener al menos {minLength} caracteres.");
        }
    }

    public static void AgainstPattern(string? value, string pattern, string parameterName)
    {
        AgainstNull(value, parameterName);
        if (!Regex.IsMatch(value!, pattern))
        {
            throw new ValidationException($"El parámetro '{parameterName}' tiene un formato inválido.");
        }
    }

    public static void AgainstNonPositive(long value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ValidationException($"El parámetro '{parameterName}' debe ser mayor que cero.");
        }
    }

    public static void AgainstNegative(long value, string parameterName)
    {
        if (value < 0)
        {
            throw new ValidationException($"El parámetro '{parameterName}' no puede ser negativo.");
        }
    }
}

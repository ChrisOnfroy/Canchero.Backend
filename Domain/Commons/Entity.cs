namespace Domain.Commons;

/// <summary>
/// Representa una entidad con identidad estable.
/// </summary>
public abstract class Entity
{
    protected Entity()
    {
        Id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    protected Entity(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Identificador único de la entidad.
    /// </summary>
    public Guid Id { get; protected init; }

    /// <summary>
    /// Fecha de creación.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Fecha de la última modificación.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Actualiza la fecha de modificación.
    /// </summary>
    protected void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Actualiza la fecha de modificación con una hora proporcionada.
    /// </summary>
    protected void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() && Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right)
        => ReferenceEquals(left, right) || (left is not null && right is not null && left.Equals(right));

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}

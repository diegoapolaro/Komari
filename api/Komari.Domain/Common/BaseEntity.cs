namespace Komari.Domain.Common;

/// <summary>
/// Base class for all domain entities providing identification, audit timestamps, and soft-delete capabilities.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    public virtual void Deactivate()
    {
        IsActive = false;
        TouchUpdated();
    }

    public virtual void Activate()
    {
        IsActive = true;
        TouchUpdated();
    }

    public void TouchUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

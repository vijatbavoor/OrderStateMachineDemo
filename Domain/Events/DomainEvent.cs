namespace OrderStateMachineDemo.Domain.Events;

/// <summary>Base class for all domain events.</summary>
public abstract class DomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public override string ToString() =>
        $"[{GetType().Name}] at {OccurredOn:yyyy-MM-dd HH:mm:ss} UTC";
}

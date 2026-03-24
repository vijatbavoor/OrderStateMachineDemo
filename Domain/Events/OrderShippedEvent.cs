namespace OrderStateMachineDemo.Domain.Events;

public sealed class OrderShippedEvent : DomainEvent
{
    public int OrderId { get; }

    public OrderShippedEvent(int orderId)
    {
        OrderId = orderId;
    }

    public override string ToString() =>
        $"[OrderShippedEvent] OrderId={OrderId} at {OccurredOn:yyyy-MM-dd HH:mm:ss} UTC";
}

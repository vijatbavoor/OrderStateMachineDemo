namespace OrderStateMachineDemo.Domain.Events;

public sealed class OrderDeliveredEvent : DomainEvent
{
    public int OrderId { get; }

    public OrderDeliveredEvent(int orderId)
    {
        OrderId = orderId;
    }

    public override string ToString() =>
        $"[OrderDeliveredEvent] OrderId={OrderId} at {OccurredOn:yyyy-MM-dd HH:mm:ss} UTC";
}

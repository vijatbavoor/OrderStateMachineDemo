namespace OrderStateMachineDemo.Domain.Events;

public sealed class PaymentFailedEvent : DomainEvent
{
    public int OrderId { get; }
    public int AttemptNumber { get; }

    public PaymentFailedEvent(int orderId, int attemptNumber)
    {
        OrderId = orderId;
        AttemptNumber = attemptNumber;
    }

    public override string ToString() =>
        $"[PaymentFailedEvent] OrderId={OrderId}, Attempt={AttemptNumber} at {OccurredOn:yyyy-MM-dd HH:mm:ss} UTC";
}

namespace OrderStateMachineDemo.Persistence;

using OrderStateMachineDemo.Domain;

/// <summary>EF Core entity that persists order state information.</summary>
public class OrderEntity
{
    public int Id { get; set; }

    /// <summary>Current state stored as a string to be human-readable in the DB.</summary>
    public string State { get; set; } = OrderState.Created.ToString();

    /// <summary>Number of payment attempts made so far.</summary>
    public int PaymentAttempts { get; set; } = 0;

    /// <summary>Required quantity for stock check.</summary>
    public int RequiredQuantity { get; set; } = 5;
}

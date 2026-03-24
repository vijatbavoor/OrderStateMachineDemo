namespace OrderStateMachineDemo.Domain;

/// <summary>All possible states in the order lifecycle.</summary>
public enum OrderState
{
    Created,
    StockChecked,
    PaymentPending,
    PaymentCompleted,
    AddressValidated,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>All triggers that drive state transitions.</summary>
public enum OrderTrigger
{
    CheckStock,
    PaymentSuccess,
    PaymentFailed,
    AddressValid,
    AddressInvalid,
    Ship,
    Deliver,
    Cancel
}

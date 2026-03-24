using OrderStateMachineDemo.Domain;
using OrderStateMachineDemo.Persistence;
using OrderStateMachineDemo.StateMachine;

// ─── Banner ──────────────────────────────────────────────────────────────────
Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║     Order State Machine Demo — Event-Driven Demo     ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine();

const int OrderId = 1;

// ─── Clean up previous run so we always start fresh ──────────────────────────
if (File.Exists("orders.db"))
{
    File.Delete("orders.db");
    Console.WriteLine("[Setup] Deleted previous orders.db for a clean run.\n");
}

// ─── Bootstrap ───────────────────────────────────────────────────────────────
using var db   = new OrdersDbContext();
var orderSm    = new OrderStateMachine(OrderId, db);
Console.WriteLine();

// ─── Simulate workflow ────────────────────────────────────────────────────────
//  CheckStock → PaymentFailed (x2) → PaymentSuccess → AddressValid → Ship → Deliver

Console.WriteLine("━━━  Step 1: Check Stock  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
orderSm.Fire(OrderTrigger.CheckStock);
Console.WriteLine();

Console.WriteLine("━━━  Step 2: Payment Fails (Attempt 1)  ━━━━━━━━━━━━━━");
orderSm.Fire(OrderTrigger.PaymentFailed);
Console.WriteLine();

Console.WriteLine("━━━  Step 3: Payment Fails (Attempt 2)  ━━━━━━━━━━━━━━");
orderSm.Fire(OrderTrigger.PaymentFailed);
Console.WriteLine();

Console.WriteLine("━━━  Step 4: Payment Succeeds  ━━━━━━━━━━━━━━━━━━━━━━━");
orderSm.Fire(OrderTrigger.PaymentSuccess);
Console.WriteLine();

Console.WriteLine("━━━  Step 5: Address Validated  ━━━━━━━━━━━━━━━━━━━━━━");
orderSm.Fire(OrderTrigger.AddressValid);
Console.WriteLine();

Console.WriteLine("━━━  Step 6: Ship Order  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
orderSm.Fire(OrderTrigger.Ship);
Console.WriteLine();

Console.WriteLine("━━━  Step 7: Deliver Order  ━━━━━━━━━━━━━━━━━━━━━━━━━━");
orderSm.Fire(OrderTrigger.Deliver);
Console.WriteLine();

// ─── Final Summary ────────────────────────────────────────────────────────────
Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║                   Final Summary                      ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine($"  Final State : {orderSm.CurrentState}");
Console.WriteLine($"  Events Published ({orderSm.PublishedEvents.Count}):");
foreach (var evt in orderSm.PublishedEvents)
{
    Console.WriteLine($"    • {evt}");
}
Console.WriteLine();
Console.WriteLine("Done. ✅");

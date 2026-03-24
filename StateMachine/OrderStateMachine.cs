namespace OrderStateMachineDemo.StateMachine;

using OrderStateMachineDemo.Domain;
using OrderStateMachineDemo.Domain.Events;
using OrderStateMachineDemo.Persistence;
using Stateless;

/// <summary>
/// Manages the order lifecycle through state transitions using the Stateless library.
/// Persists state to SQLite via EF Core and publishes domain events on key transitions.
/// </summary>
public class OrderStateMachine
{
    private const int MaxPaymentAttempts = 3;

    private readonly int _orderId;
    private readonly StateMachine<OrderState, OrderTrigger> _machine;
    private readonly OrdersDbContext _db;
    private readonly List<DomainEvent> _domainEvents = new();

    private OrderState _currentState;
    private int _paymentAttempts;

    // ── Public read properties ──────────────────────────────────────────────
    public OrderState CurrentState => _currentState;
    public IReadOnlyList<DomainEvent> PublishedEvents => _domainEvents.AsReadOnly();

    // ── Constructor ─────────────────────────────────────────────────────────
    public OrderStateMachine(int orderId, OrdersDbContext db)
    {
        _orderId = orderId;
        _db     = db;

        // Load or seed the order entity
        LoadOrSeedOrder();

        // Wire up the state machine with the current persisted state
        _machine = new StateMachine<OrderState, OrderTrigger>(
            stateAccessor:  () => _currentState,
            stateMutator:   s  => _currentState = s);

        ConfigureTransitions();

        Console.WriteLine($"[OrderStateMachine] Initialized Order #{_orderId} — State: {_currentState}, PaymentAttempts: {_paymentAttempts}");
    }

    // ── Public fire methods ─────────────────────────────────────────────────

    public void Fire(OrderTrigger trigger)
    {
        if (!_machine.CanFire(trigger))
        {
            Console.WriteLine($"[WARN] Trigger '{trigger}' is not permitted in state '{_currentState}'. Skipping.");
            return;
        }

        Console.WriteLine($"  → Firing: {trigger}  (State before: {_currentState})");
        _machine.Fire(trigger);
        Console.WriteLine($"  ✓ State after:  {_currentState}");
        PersistState();
    }

    // ── Transition configuration ────────────────────────────────────────────

    private void ConfigureTransitions()
    {
        // Created → StockChecked
        _machine.Configure(OrderState.Created)
            .Permit(OrderTrigger.CheckStock, OrderState.StockChecked)
            .Permit(OrderTrigger.Cancel, OrderState.Cancelled)
            .OnEntry(() => Console.WriteLine("[State] Order created."));

        // StockChecked → PaymentPending (first failure) or PaymentCompleted
        _machine.Configure(OrderState.StockChecked)
            .Permit(OrderTrigger.PaymentSuccess, OrderState.PaymentCompleted)
            .Permit(OrderTrigger.PaymentFailed,  OrderState.PaymentPending)
            .Permit(OrderTrigger.Cancel, OrderState.Cancelled)
            .OnEntry(() => Console.WriteLine("[State] Stock has been checked."))
            .OnEntryFrom(OrderTrigger.PaymentFailed, OnPaymentFailed); // should not happen, safety net

        // PaymentPending — retry (reentry) or escalate to Cancelled
        _machine.Configure(OrderState.PaymentPending)
            .Permit(OrderTrigger.PaymentSuccess, OrderState.PaymentCompleted)
            // Self-reentry while under the retry limit
            .PermitReentryIf(OrderTrigger.PaymentFailed,
                             () => _paymentAttempts < MaxPaymentAttempts,
                             "Max retries not reached")
            // Transition to Cancelled once limit is hit
            .PermitIf(OrderTrigger.PaymentFailed, OrderState.Cancelled,
                      () => _paymentAttempts >= MaxPaymentAttempts,
                      "Max retries reached — cancelling")
            .Permit(OrderTrigger.Cancel, OrderState.Cancelled)
            // OnEntryFrom fires on every entry into PaymentPending due to PaymentFailed
            .OnEntryFrom(OrderTrigger.PaymentFailed, OnPaymentFailed)
            .OnEntry(() => Console.WriteLine("[State] Awaiting payment retry…"));

        // PaymentCompleted → AddressValidated
        _machine.Configure(OrderState.PaymentCompleted)
            .Permit(OrderTrigger.AddressValid,   OrderState.AddressValidated)
            .Permit(OrderTrigger.AddressInvalid, OrderState.Cancelled)
            .Permit(OrderTrigger.Cancel, OrderState.Cancelled)
            .OnEntry(() => Console.WriteLine("[State] Payment completed."));

        // AddressValidated → Shipped
        _machine.Configure(OrderState.AddressValidated)
            .Permit(OrderTrigger.Ship,   OrderState.Shipped)
            .Permit(OrderTrigger.Cancel, OrderState.Cancelled)
            .OnEntry(() => Console.WriteLine("[State] Address validated."));

        // Shipped → Delivered
        _machine.Configure(OrderState.Shipped)
            .Permit(OrderTrigger.Deliver, OrderState.Delivered)
            .OnEntry(OnShipped);

        // Terminal states
        _machine.Configure(OrderState.Delivered)
            .OnEntry(OnDelivered);

        _machine.Configure(OrderState.Cancelled)
            .OnEntry(() => Console.WriteLine("[State] Order CANCELLED."));
    }

    // ── Entry callbacks ─────────────────────────────────────────────────────

    private void OnPaymentFailed()
    {
        _paymentAttempts++;
        Console.WriteLine($"[Retry] Payment failed. Attempt {_paymentAttempts}/{MaxPaymentAttempts}.");

        var evt = new PaymentFailedEvent(_orderId, _paymentAttempts);
        PublishEvent(evt);

        if (_paymentAttempts >= MaxPaymentAttempts)
        {
            Console.WriteLine($"[Retry] Maximum payment attempts ({MaxPaymentAttempts}) reached. Order will be cancelled.");
        }
    }

    private void OnShipped()
    {
        Console.WriteLine("[State] Order shipped! 🚚");
        PublishEvent(new OrderShippedEvent(_orderId));
    }

    private void OnDelivered()
    {
        Console.WriteLine("[State] Order delivered! 📦✅");
        PublishEvent(new OrderDeliveredEvent(_orderId));
    }

    // ── Domain events ───────────────────────────────────────────────────────

    private void PublishEvent(DomainEvent evt)
    {
        _domainEvents.Add(evt);
        Console.WriteLine($"[Event] Published → {evt}");
    }

    // ── Persistence ─────────────────────────────────────────────────────────

    private void LoadOrSeedOrder()
    {
        _db.Database.EnsureCreated();

        var entity = _db.Orders.Find(_orderId);
        if (entity is null)
        {
            // First run — seed the entity
            entity = new OrderEntity { Id = _orderId, State = OrderState.Created.ToString(), PaymentAttempts = 0 };
            _db.Orders.Add(entity);
            _db.SaveChanges();
            Console.WriteLine($"[DB] New order seeded — Id={_orderId}");
        }
        else
        {
            Console.WriteLine($"[DB] Order reloaded from DB — Id={_orderId}, State={entity.State}, PaymentAttempts={entity.PaymentAttempts}");
        }

        _currentState   = Enum.Parse<OrderState>(entity.State);
        _paymentAttempts = entity.PaymentAttempts;
    }

    private void PersistState()
    {
        var entity = _db.Orders.Find(_orderId);
        if (entity is null) return;

        entity.State           = _currentState.ToString();
        entity.PaymentAttempts = _paymentAttempts;
        _db.SaveChanges();

        Console.WriteLine($"[DB] Persisted — State={entity.State}, PaymentAttempts={entity.PaymentAttempts}");
    }
}

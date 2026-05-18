# Golden Examples: Untestable vs. Testable Code

Use these before-and-after structures to guide your isolated code and test generation.

## 1. Domain Service Decoupling

### Anti-Pattern (Untestable - Tied to DB, Time, and SDKs)

```javascript
class OrderService {
    shipOrder(orderId) {
        // Violates DIP: Hardcoded database connection
        const order = PostgresConnection.query("SELECT * FROM orders WHERE id = " + orderId);
        
        // Non-Deterministic: Tied to physical system clock
        if (order.shippingDate < System.DateTime.now()) {
            throw new Error("Invalid shipping window");
        }
        
        order.status = "SHIPPED";
        PostgresConnection.execute("UPDATE orders SET status = 'SHIPPED' WHERE id = " + orderId);
        
        // Side Effect: Direct global network call
        GlobalCloudLogger.send("Order shipped: " + orderId);
    }
}
```

### Refactored Solution (100% Unit-Testable Core)

```javascript
// Use Case Interactor (Decoupled & Deterministic)
class ShipOrderUseCase {
    constructor(orderRepo, telemetry) {
        this.orderRepo = orderRepo; // Injected Port Interface
        this.telemetry = telemetry; // Injected Port Interface
    }

    execute(orderId, currentEvaluationTime) {
        const order = this.orderRepo.findById(orderId);
        if (!order) throw new OrderNotFoundException();

        // Pure business logic validation on decoupled parameters
        order.ship(currentEvaluationTime);

        this.orderRepo.save(order);
        this.telemetry.logEvent("ORDER_SHIPPED", orderId.toString());

        return { id: order.id, status: order.status };
    }
}
```

---

## 2. Test Suite Execution (Strict AAA Pattern)

```javascript
class ShipOrderUseCaseTests {
    test_should_successfully_ship_order_when_date_is_within_valid_window() {
        // 1. ARRANGE
        const fixedExecutionTime = new Date("2026-05-16T12:00:00Z");
        const targetOrderId = "usr-991823";
        
        // Set up clean domain entity state
        const validOrder = new Order(targetOrderId);
        validOrder.setShippingDeadline(new Date("2026-05-20T12:00:00Z"));

        // Provision isolated test double behaviors (Stubs/Spies)
        const stubOrderRepo = new StubOrderRepository();
        stubOrderRepo.returnsOnFind(validOrder);
        
        const spyTelemetry = new SpyTelemetryPort();
        const sut = new ShipOrderUseCase(stubOrderRepo, spyTelemetry);

        // 2. ACT
        const output = sut.execute(targetOrderId, fixedExecutionTime);

        // 3. ASSERT
        assert.equal(output.status, "SHIPPED");
        assert.true(stubOrderRepo.saveWasCalledWith(validOrder));
        assert.true(spyTelemetry.eventWasLogged("ORDER_SHIPPED", targetOrderId));
    }

    test_should_throw_exception_when_shipping_deadline_has_passed() {
        // 1. ARRANGE
        const expiredExecutionTime = new Date("2026-05-25T12:00:00Z");
        const targetOrderId = "usr-991823";
        
        const expiredOrder = new Order(targetOrderId);
        expiredOrder.setShippingDeadline(new Date("2026-05-20T12:00:00Z"));

        const stubOrderRepo = new StubOrderRepository();
        stubOrderRepo.returnsOnFind(expiredOrder);
        const dummyTelemetry = new DummyTelemetryPort();
        
        const sut = new ShipOrderUseCase(stubOrderRepo, dummyTelemetry);

        // 2. ACT & ASSERT (Exception verification block)
        assert.throws(InvalidShippingWindowException, () => {
            sut.execute(targetOrderId, expiredExecutionTime);
        });
    }
}
```
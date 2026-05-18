# Golden Examples: Tactical Pattern Blueprints

Use these before-and-after structures to guide your polymorphic code generation.

## 1. The Strategy Pattern (OCP Realization)

### Anti-Pattern (Procedural Type Checking)

```javascript
class TaxCalculator {
    calculate(order) {
        if (order.region === "DOMESTIC") {
            return order.subTotal * 0.08;
        } else if (order.region === "INTERNATIONAL") {
            return (order.subTotal * 0.15) + order.customsFee;
        }
        throw new Error("Unsupported region");
    }
}
```

### Refactored Solution (Strategy Pattern Compliant)

```javascript
// 1. Core Strategy Contract
class TaxCalculationStrategy {
    calculate(order) {}
}

// 2. Concrete Strategy Implementations
class DomesticTaxStrategy extends TaxCalculationStrategy {
    calculate(order) {
        return order.subTotal * 0.08;
    }
}

class InternationalTaxStrategy extends TaxCalculationStrategy {
    calculate(order) {
        return (order.subTotal * 0.15) + order.customsFee;
    }
}

// 3. Clean Context Class
class TaxContext {
    constructor(strategies) {
        this.strategies = strategies; // Map<String, TaxCalculationStrategy>
    }

    resolveTax(order) {
        const strategy = this.strategies.get(order.region);
        if (!strategy) throw new UnsupportedRegionException();
        return strategy.calculate(order);
    }
}
```

---

## 2. The Adapter Pattern (DIP & ISP Realization)

### Anti-Pattern (Leakage of Third-Party Signature)

```javascript
import { LegacyThirdPartyVideoSDK } from "volatile-video-library";

class VideoStreamer {
    constructor() {
        this.sdk = new LegacyThirdPartyVideoSDK(); // Hard infrastructure coupling
    }

    play(videoId) {
        this.sdk.initializeConnectionContext();
        return this.sdk.fetchRawBlobPayload(videoId.toString(), 1080);
    }
}
```

### Refactored Solution (Adapter Pattern Compliant)

```javascript
// 1. High-Level Target Interface (Domain Port)
class VideoStreamPort {
    playStream(videoId) {}
}

// 2. Concrete Adapter encapsulating third-party detail
class VideoProviderAdapter extends VideoStreamPort {
    constructor(externalSdk) {
        super();
        this.externalSdk = externalSdk;
    }

    playStream(videoId) {
        this.externalSdk.initializeConnectionContext();
        const rawBlob = this.externalSdk.fetchRawBlobPayload(videoId.toString(), 1080);
        return StreamBuffer.fromRawBlob(rawBlob);
    }
}
```

---

## 3. The Decorator Pattern (SRP Realization)

### Anti-Pattern (Cross-Cutting Boilerplate Bleeding)

```javascript
class RealOrderProcessor {
    process(orderId) {
        console.time("order_process_latency");
        try {
            const order = OrderRepository.find(orderId);
            order.markAsDispatched();
            OrderRepository.save(order);
            Metrics.increment("order_success");
        } catch (error) {
            Metrics.increment("order_failure");
            throw error;
        } finally {
            console.timeEnd("order_process_latency");
        }
    }
}
```

### Refactored Solution (Decorator Pattern Compliant)

```javascript
// 1. Common Interface
class OrderProcessor {
    process(orderId) {}
}

// 2. Pure Core Component (SRP - strictly business logic)
class RealOrderProcessor extends OrderProcessor {
    process(orderId) {
        const order = OrderRepository.find(orderId);
        order.markAsDispatched();
        OrderRepository.save(order);
    }
}

// 3. Decorator Component wrapping the core with cross-cutting logic
class TelemetryOrderProcessorDecorator extends OrderProcessor {
    constructor(wrappedProcessor, telemetry) {
        super();
        this.wrappedProcessor = wrappedProcessor;
        this.telemetry = telemetry;
    }

    process(orderId) {
        this.telemetry.startTimer("order_process_latency");
        try {
            this.wrappedProcessor.process(orderId);
            this.telemetry.incrementCounter("order_process_success");
        } catch (error) {
            this.telemetry.incrementCounter("order_process_failure");
            throw error;
        } finally {
            this.telemetry.stopTimer("order_process_latency");
        }
    }
}
```
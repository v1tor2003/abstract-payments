# SOLID Refactoring Reference Blueprints

Use these before-and-after structural pairs to guide your code generation.

## 1. Single Responsibility Principle (SRP)

### Anti-Pattern (Violating SRP)

```javascript
class UserService {
    registerUser(data) {
        if (!data.email.includes("@")) throw new Error("Invalid email");
        Database.execute("INSERT INTO users (email) VALUES (" + data.email + ")");
        let smtpClient = new SmtpClient("smtp.provider.com");
        smtpClient.sendWelcomeEmail(data.email);
    }
}
```

### Refactored Solution (SRP Compliant)
```javascript
class UserValidator {
    validate(data) {
        if (!data.email.includes("@")) throw new Error("Invalid email");
    }
}

class UserRepository {
    save(user) {
        Database.execute("INSERT INTO users...", [user.email]);
    }
}

class NotificationService {
    sendWelcome(email) {
        this.client.send(email, "Welcome!");
    }
}

class UserRegistrationCoordinator {
    constructor(validator, repo, notifier) {
        this.validator = validator;
        this.repo = repo;
        this.notifier = notifier;
    }
    
    execute(data) {
        this.validator.validate(data);
        let user = { email: data.email };
        this.repo.save(user);
        this.notifier.sendWelcome(user.email);
    }
}
```

---

## 2. Open/Closed Principle (OCP)

### Anti-Pattern (Violating OCP)

```javascript
class PaymentProcessor {
    process(payment) {
        if (payment.provider === "STRIPE") {
            this.executeStripe(payment);
        } else if (payment.provider === "PAYPAL") {
            this.executePaypal(payment);
        }
    }
}
```

### Refactored Solution (OCP Compliant)

```javascript
class PaymentProcessor {
    constructor() {
        this.gateways = new Map();
    }

    registerGateway(name, gateway) {
        this.gateways.set(name, gateway);
    }

    process(payment) {
        let gateway = this.gateways.get(payment.provider);
        if (!gateway) throw new Error("Unsupported gateway");
        gateway.execute(payment);
    }
}
```

---

## 3. Liskov Substitution Principle (LSP)

### Anti-Pattern (Violating LSP)

```javascript
class FileRepository {
    readData(path) { return FileSystem.read(path); }
    writeData(path, data) { FileSystem.write(path, data); }
}

class ReadOnlyFileRepository extends FileRepository {
    writeData(path, data) {
        throw new Error("Cannot write to a read-only repository.");
    }
}
```

### Refactored Solution (LSP Compliant)

```javascript
// Segregate read and write behaviors to maintain behavioral safety
class ReadOnlyFileRepository {
    readData(path) { return FileSystem.read(path); }
}

class StandardFileRepository extends ReadOnlyFileRepository {
    writeData(path, data) { FileSystem.write(path, data); }
}
```

---

## 4. Interface Segregation Principle (ISP)

### Anti-Pattern (Violating ISP)

```javascript
class EconomicPrinter {
    printDocument() { /* print */ }
    scanDocument() { throw new Error("Not supported"); } // Dead code stub
}
```

### Refactored Solution (ISP Compliant)

```javascript
// Consumer interfaces are granular
class EconomicPrinter {
    printDocument() { /* print logic only */ }
}

class AllInOneOfficeHub {
    printDocument() { /* print */ }
    scanDocument() { /* scan */ }
}
```

---

## 5. Dependency Inversion Principle (DIP)

### Anti-Pattern (Violating DIP)

```javascript
import { PostgresEngine } from "./Postgres";

class OrderProcessor {
    constructor() {
        this.database = new PostgresEngine(); // Hard coupling
    }
    complete(orderId) {
        this.database.query("UPDATE orders SET status = 'DONE' WHERE id =?", [orderId]);
    }
}
```

### Refactored Solution (DIP Compliant)

```javascript
// Interface declared in High-Level Core domain
class OrderPersistencePort {
    updateStatus(id, status) {}
}

class OrderProcessor {
    constructor(persistencePort) {
        this.persistence = persistencePort; // Dynamic Injection
    }
    complete(orderId) {
        this.persistence.updateStatus(orderId, "DONE");
    }
}
```
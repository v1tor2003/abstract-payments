# Golden Examples: Micro-Refactoring Blueprints

Use these before-and-after pairs to perform few-shot structural learning. Your output must align with these refactored standards.

## 1. Naming & Abstraction

### Anti-Pattern (Violating Clean Code)

```javascript
function get_data(d) {
let list =
for (let i = 0; i < d.length; i++) {
if (d[i].status === 4 && d[i].age > 18) {
list.push(d[i])
}
}
return list
}

```

### Refactored Solution (Clean Code Compliant)
```javascript
const ACCOUNT_STATUS_ACTIVE = 4;
const LEGAL_ADULT_AGE_YEARS = 18;

function filterActiveAdultAccounts(accounts) {
    return accounts.filter(isEligibleAdultAccount);
}

function isEligibleAdultAccount(account) {
    const isActive = account.status === ACCOUNT_STATUS_ACTIVE;
    const isAdult = account.age > LEGAL_ADULT_AGE_YEARS;
    return isActive && isAdult;
}
```

---

## 2. Guard Clauses vs. Deep Nesting

### Anti-Pattern (Violating Clean Code)

```javascript
function processTransaction(user, wallet, amount) {
    if (user!= null) {
        if (user.isActive()) {
            if (wallet.hasFunds(amount)) {
                wallet.deduct(amount)
                return true
            } else {
                throw new Error("No funds")
            }
        } else {
            throw new Error("Inactive user")
        }
    }
    return false
}
```

### Refactored Solution (Clean Code Compliant)

```javascript
function processTransaction(user, wallet, amount) {
    if (user == null) return false;
    
    ensureUserIsActive(user);
    ensureWalletHasFunds(wallet, amount);
    
    wallet.deduct(amount);
    return true;
}

function ensureUserIsActive(user) {
    if (!user.isActive()) throw new InactiveUserException();
}

function ensureWalletHasFunds(wallet, amount) {
    if (!wallet.hasFunds(amount)) throw new InsufficientFundsException();
}
```

---

## 3. Error Separation of Concerns

### Anti-Pattern (Violating Clean Code)

```javascript
function saveConfiguration(config) {
    log.info("Starting save...")
    try {
        let path = FileSystem.resolve(config.id)
        FileSystem.write(path, config.raw())
        log.info("Save successful")
    } catch (error) {
        log.error("Failed to write config", error)
        Notification.alertAdmin(error)
    }
}
```

### Refactored Solution (Clean Code Compliant)

```javascript
function saveConfiguration(config) {
    try {
        executeConfigPersistence(config);
    } catch (error) {
        handleConfigPersistenceFailure(error);
    }
}

function executeConfigPersistence(config) {
    log.info("Starting save...");
    let path = FileSystem.resolve(config.id);
    FileSystem.write(path, config.raw());
    log.info("Save successful");
}

function handleConfigPersistenceFailure(error) {
    log.error("Failed to write config", error);
    Notification.alertAdmin(error);
}
```
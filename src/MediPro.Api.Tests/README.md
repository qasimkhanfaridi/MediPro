# MediPro.Api.Tests

Unit tests for the MediPro API backend (.NET 9).

## ?? Test Coverage

### Test Statistics
- **Total Tests:** 49
- **Test Status:** ? All Passing
- **Test Framework:** xUnit
- **Assertion Library:** FluentAssertions
- **Mocking:** In-Memory Database (EF Core)

---

## ?? Test Categories

### 1. Domain Logic Tests (`Domain/`)
- **OrderStatusRulesTests** (23 tests)
  - Order status transition validation
  - Terminal status handling
  - Status workflow rules
  - Allowed transitions summary

### 2. Service Tests (`Services/`)
- **TokenServiceTests** (6 tests)
  - JWT token creation
  - Claims validation
  - Token expiration
  - Admin vs Store user tokens
  - Store ID claim handling

- **AdminAlertServiceTests** (7 tests)
  - Order notification creation
  - Store registration notifications
  - Multi-admin notifications
  - Tenant isolation
  - Admin filtering

### 3. Extension Tests (`Extensions/`)
- **ClaimsPrincipalExtensionsTests** (13 tests)
  - User ID extraction
  - Tenant ID extraction
  - Store ID extraction
  - Null handling
  - Invalid GUID handling
  - Multiple claim types

---

## ?? Running the Tests

### Quick Run
```powershell
cd B:\MediPro\src\MediPro.Api.Tests
.\run-tests.ps1
```

### Manual Commands

**Run all tests:**
```powershell
dotnet test
```

**Run with detailed output:**
```powershell
dotnet test --logger "console;verbosity=detailed"
```

**Run specific test class:**
```powershell
dotnet test --filter "FullyQualifiedName~OrderStatusRulesTests"
```

**Run tests with coverage (requires coverlet):**
```powershell
dotnet test /p:CollectCoverage=true
```

---

## ?? Test Results Summary

```
Test Run Successful.
Total tests: 49
     Passed: 49
     Failed: 0
     Skipped: 0
Total time: ~2-3 seconds
```

---

## ?? Test Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| xUnit | 2.9.0+ | Test framework |
| FluentAssertions | 7.0.0+ | Fluent assertion library |
| Moq | 4.20.0+ | Mocking framework |
| EF Core InMemory | 9.0.12 | In-memory database for testing |

---

## ?? Test Structure

### Domain Tests
Focus on business logic without dependencies:
- Order status transitions
- Validation rules
- Domain calculations

### Service Tests
Test service layer with in-memory database:
- Token generation
- Notification creation
- Database interactions
- Multi-tenancy logic

### Extension Tests
Test utility extension methods:
- Claims parsing
- Null handling
- Data extraction

---

## ? What's Tested

### ? Core Business Logic
- Order status workflow (23 tests)
- All valid/invalid transitions
- Terminal status behavior

### ? Authentication & Authorization
- JWT token creation
- Claims structure
- Role-based tokens
- Token expiration

### ? Notifications System
- Admin notifications
- Order alerts
- Store registration alerts
- Multi-tenant isolation

### ? Extension Methods
- Claims principal parsing
- Safe GUID extraction
- Null/invalid handling

---

## ?? Testing Best Practices

These tests follow:
- **Arrange-Act-Assert** (AAA) pattern
- **Clear test names** describing what is being tested
- **Isolated tests** - each test is independent
- **In-memory database** - fast, no external dependencies
- **FluentAssertions** - readable, expressive assertions
- **Theory/InlineData** - parameterized tests for multiple scenarios

---

## ?? What's NOT Tested (Yet)

These areas could benefit from additional tests:

### Controllers
- API endpoint behavior
- HTTP status codes
- Request/response validation
- Authorization checks

### Excel Import Service
- File parsing
- Validation logic
- Error handling
- Data transformation

### Data Seeding
- Demo data creation
- Database initialization
- Seed data integrity

---

## ?? Adding New Tests

### 1. Create test file in appropriate folder:
```csharp
// MediPro.Api.Tests/Services/MyNewServiceTests.cs
using FluentAssertions;
using Xunit;

namespace MediPro.Api.Tests.Services;

public class MyNewServiceTests
{
    [Fact]
    public void MyTest_Description_ExpectedBehavior()
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

### 2. Run tests:
```powershell
dotnet test
```

### 3. Tests are automatically discovered by xUnit

---

## ?? Debugging Tests

### Run single test:
```powershell
dotnet test --filter "FullyQualifiedName~MyTest_Description_ExpectedBehavior"
```

### Debug in Visual Studio:
1. Open Test Explorer (Test > Test Explorer)
2. Right-click test ? Debug
3. Set breakpoints as needed

### Debug in VS Code:
1. Install C# Dev Kit extension
2. Set breakpoints in test file
3. Run "Debug Test" from CodeLens

---

## ?? Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [EF Core Testing](https://learn.microsoft.com/en-us/ef/core/testing/)
- [Unit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

## ? Test Maintenance

These tests should be run:
- ? Before every commit
- ? In CI/CD pipeline
- ? After dependency updates
- ? When fixing bugs (add regression tests)
- ? When adding new features

---

**All tests are passing! Happy testing! ??**

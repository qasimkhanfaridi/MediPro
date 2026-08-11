# ?? MediPro - Unit Testing Guide

## ? Test Suite Status

| Project | Tests | Status | Coverage |
|---------|-------|--------|----------|
| **MediPro.Api.Tests** | 49 | ? All Passing | Core Business Logic |

---

## ?? Quick Start - Run Tests

### Option 1: PowerShell Script (Easiest)
```powershell
cd B:\MediPro\src\MediPro.Api.Tests
.\run-tests.ps1
```

### Option 2: Direct Command
```powershell
cd B:\MediPro\src\MediPro.Api.Tests
dotnet test
```

### Option 3: From Solution Root
```powershell
cd B:\MediPro\src
dotnet test
```

---

## ?? Test Coverage Summary

### ? **What's Tested (49 Tests)**

#### 1. **Domain Logic** (23 tests)
- ? Order status transitions
- ? Workflow validation
- ? Terminal status behavior
- ? Status transition rules

#### 2. **Token Service** (6 tests)
- ? JWT token generation
- ? Claims structure
- ? Token expiration
- ? Role-based tokens
- ? Store ID claims

#### 3. **Admin Alert Service** (7 tests)
- ? Order notifications
- ? Store registration alerts
- ? Multi-admin handling
- ? Tenant isolation
- ? Notification creation

#### 4. **Extension Methods** (13 tests)
- ? Claims parsing
- ? User ID extraction
- ? Tenant ID extraction
- ? Store ID extraction
- ? Null/invalid handling

---

## ?? Test Results

```
Test Run Successful.
Total tests: 49
     Passed: 49  ?
     Failed: 0
     Skipped: 0
Total time: ~2-3 seconds
```

---

## ?? Test Technologies

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Framework** | xUnit | Test execution |
| **Assertions** | FluentAssertions | Readable assertions |
| **Database** | EF Core InMemory | Fast, isolated tests |
| **Mocking** | Moq | Mock dependencies |
| **Target** | .NET 9 | Runtime |

---

## ?? Test Project Structure

```
MediPro.Api.Tests/
??? Domain/
?   ??? OrderStatusRulesTests.cs      (23 tests)
??? Services/
?   ??? TokenServiceTests.cs          (6 tests)
?   ??? AdminAlertServiceTests.cs     (7 tests)
??? Extensions/
?   ??? ClaimsPrincipalExtensionsTests.cs (13 tests)
??? run-tests.ps1                     (Test runner script)
??? README.md                         (Detailed documentation)
```

---

## ?? Test Examples

### Example 1: Domain Logic Test
```csharp
[Fact]
public void CanTransition_FromSubmitted_ToConfirmed_ReturnsTrue()
{
    // Arrange
    var from = OrderStatus.Submitted;
    var to = OrderStatus.Confirmed;

    // Act
    var result = OrderStatusRules.CanTransition(from, to);

    // Assert
    result.Should().BeTrue();
}
```

### Example 2: Service Test with InMemory DB
```csharp
[Fact]
public async Task NotifyNewOrderSubmittedAsync_WithAdminUsers_CreatesNotifications()
{
    // Arrange
    await using var context = CreateInMemoryContext();
    var service = new AdminAlertService(context);
    // ... setup test data

    // Act
    await service.NotifyNewOrderSubmittedAsync(...);

    // Assert
    var notifications = await context.AdminNotifications.ToListAsync();
    notifications.Should().HaveCount(2);
}
```

### Example 3: Parameterized Test
```csharp
[Theory]
[InlineData(OrderStatus.Submitted, OrderStatus.Confirmed)]
[InlineData(OrderStatus.Submitted, OrderStatus.OnHold)]
[InlineData(OrderStatus.Submitted, OrderStatus.Rejected)]
public void CanTransition_ValidTransitions_ReturnsTrue(OrderStatus from, OrderStatus to)
{
    var result = OrderStatusRules.CanTransition(from, to);
    result.Should().BeTrue();
}
```

---

## ?? Running Tests in Different Ways

### Run All Tests
```powershell
dotnet test
```

### Run with Detailed Output
```powershell
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test Class
```powershell
dotnet test --filter "FullyQualifiedName~OrderStatusRulesTests"
```

### Run Tests by Category
```powershell
# Run all service tests
dotnet test --filter "FullyQualifiedName~Services"

# Run all domain tests
dotnet test --filter "FullyQualifiedName~Domain"
```

### Watch Mode (Auto-run on changes)
```powershell
dotnet watch test
```

---

## ?? Integration with Development Workflow

### 1. Before Committing Code
```powershell
cd B:\MediPro\src\MediPro.Api.Tests
dotnet test
```
All tests should pass before committing.

### 2. After Pulling Changes
```powershell
dotnet test
```
Verify changes didn't break existing functionality.

### 3. When Adding New Features
1. Write tests first (TDD approach)
2. Implement feature
3. Run tests to verify
4. Refactor with confidence

---

## ?? Test Coverage Goals

### ? Currently Covered (High Priority)
- Core business logic (OrderStatusRules)
- Authentication tokens (TokenService)
- Notifications (AdminAlertService)
- Claims parsing (Extensions)

### ?? Should Be Added (Medium Priority)
- **Controllers:**
  - AuthController (login, register)
  - OrdersController (CRUD operations)
  - ProductsController (catalog operations)
  - CartController (cart management)

- **Services:**
  - ProductExcelImporter (file parsing)
  - Data seeders validation

### ?? Nice to Have (Low Priority)
- Integration tests
- API endpoint tests
- Database migration tests
- Performance tests

---

## ?? When Tests Fail

### Step 1: Read the Error Message
Test output shows:
- Which test failed
- Expected vs actual values
- Stack trace

### Step 2: Run Failed Test in Isolation
```powershell
dotnet test --filter "FullyQualifiedName~FailedTestName"
```

### Step 3: Debug the Test
- Set breakpoints in test file
- Debug using VS/VS Code
- Inspect variables and state

### Step 4: Fix and Re-run
```powershell
dotnet test
```

---

## ?? Resources

### Documentation
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Guide](https://fluentassertions.com/)
- [EF Core Testing](https://learn.microsoft.com/en-us/ef/core/testing/)

### Testing Best Practices
- ? Each test should test one thing
- ? Tests should be independent
- ? Use descriptive test names
- ? Follow Arrange-Act-Assert pattern
- ? Keep tests fast (< 100ms each)

---

## ?? Next Steps

1. **Run the tests** to verify setup:
   ```powershell
   cd B:\MediPro\src\MediPro.Api.Tests
   .\run-tests.ps1
   ```

2. **Review test code** to understand patterns:
   - See `MediPro.Api.Tests/README.md` for details

3. **Add more tests** as you develop:
   - Controller tests
   - Integration tests
   - Edge cases

4. **Integrate with CI/CD**:
   - Run tests automatically on commits
   - Block merges if tests fail

---

## ? Benefits of These Tests

? **Confidence** - Refactor without fear
? **Documentation** - Tests show how code should behave  
? **Regression Prevention** - Catch bugs early
? **Design Feedback** - Tests reveal design issues
? **Fast Feedback** - All tests run in ~3 seconds

---

**All 49 tests passing! Ready for development! ??**

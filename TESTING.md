# Testing Guide

## Backend Testing (.NET 8 + xUnit)

### Overview
- **Test Framework:** xUnit
- **Target Coverage:** >90%
- **Location:** `backend/tests/CdbCalc.Application.Tests/`

### Test Scenarios

#### 1. Input Validation
```csharp
// Throws ArgumentException for invalid inputs
- Zero initial value
- Negative initial value
- Zero months
- Negative months
```

#### 2. Income Tax Rates (All Brackets)
```csharp
// Faixa 1: Até 6 meses → 22.5%
CdbCalculatorUseCase.Execute(1000, 3)
CdbCalculatorUseCase.Execute(1000, 6)

// Faixa 2: 7-12 meses → 20%
CdbCalculatorUseCase.Execute(1000, 12)

// Faixa 3: 13-24 meses → 17.5%
CdbCalculatorUseCase.Execute(1000, 24)

// Faixa 4: 25+ meses → 15%
CdbCalculatorUseCase.Execute(1000, 36)
```

#### 3. Calculation Correctness
```csharp
// Verify monthly compounding
// Verify rounding to 2 decimals
// Verify NetValue = GrossValue - IncomeTax
// Verify profit calculation
```

### Running Tests

**All tests:**
```bash
cd backend
dotnet test
```

**Specific test class:**
```bash
dotnet test --filter ClassName=CdbCalculatorUseCaseTests
```

**With coverage:**
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover /p:Exclude="[*Tests]*"
```

**Watch mode (automatic rerun):**
```bash
dotnet watch test
```

### Expected Output
```
Test Run Successful.
Total tests: 15
     Passed: 15
     Failed: 0
     Skipped: 0
Time Taken: XX.XXs
```

### Coverage Report
- Located in: `backend/tests/CdbCalc.Application.Tests/coverage/`
- Open `index.htm` in browser
- Target: >90% line coverage

## Frontend Testing (Angular 17+ + Jasmine/Karma)

### Overview
- **Test Framework:** Jasmine + Karma
- **Target Coverage:** >80%
- **Location:** `frontend/src/app/**/*.spec.ts`

### Test Files

#### 1. `cdb-calculator.service.spec.ts`
```typescript
- Service creation
- POST request with correct parameters
- Response parsing
- Error handling (400 Bad Request)
- Observable subscription
```

#### 2. `loading.service.spec.ts`
```typescript
- Service creation
- Signal initialization (false)
- Signal update (set true/false)
- Reactivity
```

#### 3. `correlation-id.interceptor.spec.ts`
```typescript
- Header injection on every request
- UUID generation (valid format)
- Session reuse (same ID across requests)
- Header presence verification
```

#### 4. `loading.interceptor.spec.ts`
```typescript
- Set isLoading to true during request
- Set isLoading to false on completion
- Handle errors gracefully
- Finalize callback execution
```

### Running Tests

**All tests (single run):**
```bash
cd frontend
npm test -- --watch=false
```

**Watch mode (auto-rerun on changes):**
```bash
npm test
```

**Coverage report:**
```bash
npm test -- --code-coverage --watch=false
```

**Chrome headless (CI):**
```bash
npm test -- --watch=false --browsers=ChromeHeadless
```

**Debug mode (inspect in Chrome):**
```bash
npm test -- --browsers=Chrome --watch=true
# Browser opens at http://localhost:9876/debug.html
```

### Expected Output
```
Chrome XX.X.XXXX.XXX (Linux/Windows/Mac): Executed XX of XX SUCCESS

TOTAL: XX SUCCESS
```

### Coverage Report
- Location: `frontend/coverage/cdb-calculator-frontend/`
- Open `index.html` in browser
- Target: >80% line + branch coverage

### Component Testing
While no component .spec.ts is provided (simple standalone component),
tests for services and interceptors cover integration scenarios.

For component testing:
```typescript
// Example structure for future component tests
TestBed.configureTestingModule({
  imports: [CalculatorComponent, HttpClientTestingModule],
  providers: [CdbCalculatorService, LoadingService]
});
const component = TestBed.createComponent(CalculatorComponent);
expect(component).toBeTruthy();
```

## Integration Testing (Manual)

### Backend API
```bash
# Test calculation endpoint
curl -X POST http://localhost:8080/api/cdbcalculator/calculate \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: test-123" \
  -d '{"initialValue": 1000, "months": 12}'

# Expected Response
{
  "initialValue": 1000,
  "months": 12,
  "grossValue": 1109.02,
  "incomeTax": 21.80,
  "netValue": 1087.22
}

# Verify Correlation ID in response header
curl -i -X POST http://localhost:8080/api/cdbcalculator/calculate \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: test-123" \
  -d '{"initialValue": 1000, "months": 12}'
# Look for: X-Correlation-ID: test-123 in response
```

### Frontend Form Validation
1. Open http://localhost
2. Leave inputs empty → button disabled
3. Enter invalid value (0 or negative) → error message shown
4. Enter valid values (1000, 12) → button enabled
5. Click Calculate → loading overlay appears
6. Results card displays after response

### End-to-End Flow
1. User enters 5000 BRL, 6 months
2. Frontend sends request with Correlation ID
3. Backend logs with CorrelationId in JSON
4. Response returns 5000, 6, ~5545, ~122, ~5423
5. Card displays results
6. Check Grafana logs with that Correlation ID

## CI/CD Considerations

### GitHub Actions Example
```yaml
- name: Run backend tests
  run: dotnet test backend

- name: Run frontend tests
  run: npm test -- --watch=false
  working-directory: frontend

- name: Upload coverage
  uses: codecov/codecov-action@v3
```

### Local Pre-commit
```bash
# Backend
cd backend && dotnet test && cd ..

# Frontend
cd frontend && npm test -- --watch=false && cd ..
```

## Debugging Tests

### Backend
```bash
# Debug single test
dotnet test --filter Name~SpecificTestName -- RunConfiguration.DebuggerEnabled=true

# Log output
dotnet test --logger "console;verbosity=detailed"
```

### Frontend
```bash
// Add console.log in test
it('should test something', () => {
  console.log('Debug info:', myService.someMethod());
  expect(true).toBe(true);
});

// Run and check browser console (http://localhost:9876/debug.html)
npm test -- --browsers=Chrome --watch=true
```

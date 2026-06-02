# Architecture Overview

## Frontend Architecture

### Component Structure (LIFT Principle)
```
app/
├── components/
│   └── calculator/
│       └── calculator.component.ts (standalone)
├── services/
│   ├── cdb-calculator.service.ts
│   └── loading.service.ts
└── interceptors/
    ├── correlation-id.interceptor.ts
    └── loading.interceptor.ts
```

### State Management
- **Signals API** (not Redux/NgRx)
- `LoadingService` exposes `isLoading` signal
- Components consume via `toSignal()` or direct signal access
- Reactive by default, no additional RxJS subscribe layer

### Communication
- **HTTP Interceptors (Functional)**
  - CorrelationId: Injects `X-Correlation-ID` header
  - Loading: Updates signal during requests
- **HttpClient:** Observable-based, compatible with Signals

## Backend Architecture

### Ports & Adapters (Hexagonal)

```
Domain (innermost)
  ├── ICdbCalculatorUseCase (Port interface)
  ├── CdbCalculation (Domain model + logic)
  └── No external dependencies

Application (middle)
  └── CdbCalculatorUseCase (Use case implementation)

Adapters (outermost)
  ├── Primary (HTTP)
  │   ├── CdbCalculatorController
  │   └── CorrelationIdMiddleware
  └── Infrastructure
      ├── Serilog logging
      └── Swagger/OpenAPI
```

### Dependency Flow
- `Program.cs` injects `ICdbCalculatorUseCase` → `CdbCalculatorUseCase`
- Controller depends on interface, not concrete class
- **SOLID:** DIP (Dependency Inversion), ISP (Interface Segregation)

## Observability

### Correlation ID Flow
1. Frontend: `CorrelationIdInterceptor` generates or reuses UUID
2. Request: Sent with `X-Correlation-ID` header
3. Backend: `CorrelationIdMiddleware` captures and injects into LogContext
4. Logging: Serilog includes `CorrelationId` in JSON structured logs
5. Response: Header returned to client
6. Loki: Logs ingested with `CorrelationId` field for querying

### Logging Chain
- Serilog outputs JSON to console
- Promtail scrapes container stdout
- Loki stores indexed logs
- Grafana queries Loki with LogQL

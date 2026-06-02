# Development Setup

## Local Development Environment

### Prerequisites
- .NET 8 SDK
- Node.js 20+ with npm 10+
- Docker & Docker Compose (for observability stack)

## Backend Development

### Setup
```bash
cd backend

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run migrations/setup
# (Not needed for this project - no database)

# Run development server
dotnet run --project src/CdbCalc.Adapters.Primary.WebApi
```

### Development Server
- URL: `http://localhost:5000` (HTTPS) or `http://localhost:5001` (HTTP)
- Swagger UI: Available at `/swagger`
- Hot reload: Automatic on file changes

### Running Tests
```bash
# All tests
dotnet test

# Watch mode
dotnet watch test

# With coverage
dotnet test /p:CollectCoverage=true

# Specific test
dotnet test --filter Name~TestName
```

### Common Tasks
```bash
# Build for production
dotnet build -c Release

# Clean build
dotnet clean && dotnet build

# Run single project
dotnet run --project src/CdbCalc.Adapters.Primary.WebApi

# Format code
dotnet format
```

## Frontend Development

### Setup
```bash
cd frontend

# Install dependencies
npm install

# Verify Angular CLI
npx ng version
```

### Development Server
```bash
# Start dev server
npm start
# or
ng serve

# URL: http://localhost:4200
# Auto-reload on file changes
```

### Running Tests
```bash
# Watch mode (auto-rerun)
npm test

# Single run
npm test -- --watch=false

# With coverage
npm test -- --code-coverage --watch=false

# Specific browser
npm test -- --browsers=ChromeHeadless
```

### Building
```bash
# Development build (non-optimized)
ng build

# Production build (optimized)
ng build --configuration production

# Output: frontend/dist/cdb-calculator-frontend
```

### Code Quality
```bash
# Lint (if configured)
ng lint

# Format code (prettier if configured)
npm run format
```

## Docker Development

### Build Images Locally
```bash
# Backend
docker build -f backend/Dockerfile -t cdb-backend:dev backend/

# Frontend
docker build -f frontend/Dockerfile -t cdb-frontend:dev frontend/

# Both via compose
docker-compose build
```

### Run Full Stack
```bash
# Start all services
docker-compose up

# Rebuild and start
docker-compose up --build

# Background mode
docker-compose up -d

# View logs
docker-compose logs -f backend
docker-compose logs -f frontend

# Stop services
docker-compose down

# Clean everything
docker-compose down -v
```

### Individual Service Development
```bash
# Start only backend + observability
docker-compose up -d cdb-backend loki promtail grafana

# Start backend locally, frontend in Docker
npm start
docker-compose up cdb-frontend

# Start frontend locally, backend in Docker
cd frontend && npm start
docker-compose up cdb-backend
```

## IDE Setup

### Visual Studio Code

#### Extensions
- C# (powered by OmniSharp)
- Angular Language Service
- Docker
- REST Client (for API testing)
- GitLens

#### Launch Configuration (.vscode/launch.json)
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Backend",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/backend/bin/Debug/net8.0/CdbCalc.Adapters.Primary.WebApi.dll",
      "args": [],
      "cwd": "${workspaceFolder}/backend",
      "stopAtEntry": false,
      "console": "internalConsole"
    },
    {
      "name": "Angular Frontend",
      "type": "chrome",
      "request": "launch",
      "url": "http://localhost:4200",
      "webRoot": "${workspaceFolder}/frontend",
      "sourceMapPathOverride": {
        "webpack:///./~/*": "${webRoot}/node_modules/*",
        "webpack:///*": "${webRoot}/*"
      }
    }
  ]
}
```

#### Tasks Configuration (.vscode/tasks.json)
```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "Build Backend",
      "type": "shell",
      "command": "dotnet",
      "args": ["build", "backend"],
      "group": "build"
    },
    {
      "label": "Run Backend",
      "type": "shell",
      "command": "dotnet",
      "args": ["run", "--project", "backend/src/CdbCalc.Adapters.Primary.WebApi"],
      "group": "test",
      "isBackground": true
    },
    {
      "label": "Run Frontend",
      "type": "shell",
      "command": "npm",
      "args": ["start"],
      "cwd": "frontend",
      "group": "test",
      "isBackground": true
    }
  ]
}
```

### Visual Studio 2022

1. Open `backend/CdbCalc.sln`
2. Set startup project: `CdbCalc.Adapters.Primary.WebApi`
3. Debug → Start Debugging (F5)
4. API available at `https://localhost:5000`

### JetBrains Rider

1. Open `backend/CdbCalc.sln`
2. Click Run → Run 'CdbCalc.Adapters.Primary.WebApi'
3. Navigate to `http://localhost:5000`

## API Testing

### cURL
```bash
# Test calculation
curl -X POST http://localhost:8080/api/cdbcalculator/calculate \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: dev-test-123" \
  -d '{"initialValue": 1000, "months": 12}' \
  | jq '.'
```

### REST Client (VS Code)
Create `requests.http`:
```http
### Calculate CDB
POST http://localhost:8080/api/cdbcalculator/calculate
Content-Type: application/json
X-Correlation-ID: dev-test-123

{
  "initialValue": 1000,
  "months": 12
}

### Check Health
GET http://localhost:8080/health
```

### Postman
1. Create collection
2. Add POST `http://localhost:8080/api/cdbcalculator/calculate`
3. Body (JSON): `{"initialValue": 1000, "months": 12}`
4. Header: `X-Correlation-ID: dev-test-123`

## Debugging

### Backend
```csharp
// Add breakpoint in Visual Studio/Rider
// Or use console output
Console.WriteLine($"Debug: {variable}");

// Or Serilog
logger.Information("Debug: {@Variable}", variable);
```

### Frontend
```typescript
// Console log
console.log('Debug:', value);

// Browser DevTools F12
// Debugger in VS Code
```

### Docker
```bash
# View container logs
docker-compose logs cdb-backend

# Execute command in container
docker exec -it cdb-backend sh

# Check container health
docker ps --format "table {{.Names}}\t{{.Status}}"
```

## Database/State (Not Used)

This project has no database layer. All calculations are:
- Stateless
- In-memory
- Request-response based

## Environment Variables

### Backend
- `ASPNETCORE_ENVIRONMENT` = Development/Production
- `ASPNETCORE_URLS` = http://+:5000 (Dev) or http://+:8080 (Docker)

### Frontend
- Build-time only via `environments/*.ts` files
- No runtime env vars (SPA limitation)

## Performance Tips

### Backend
- Use Release build for benchmarks: `dotnet build -c Release`
- Profile with: `dotnet trace`
- Memory profiling: `dotnet-counters`

### Frontend
- Check bundle size: `ng build --stats-json && webpack-bundle-analyzer dist/*/stats.json`
- Performance in DevTools → Lighthouse
- Minimize: `ng build --aot --optimization`

## Git Workflow

```bash
# Clone repo
git clone <repo-url>
cd cdb-calculator

# Create feature branch
git checkout -b feature/new-feature

# Make changes, commit
git commit -m "feat: add new feature"

# Push
git push origin feature/new-feature

# Create pull request
# After review, merge to main
```

## Documentation

- Architecture: `ARCHITECTURE.md`
- Testing: `TESTING.md`
- Troubleshooting: `TROUBLESHOOTING.md`
- Main README: `README.md`

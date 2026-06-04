# CDB B3 Calculator

Monorepo para calculadora de investimentos em CDB (Certificados de Depósito Bancário) da B3, desenvolvida em **.NET 8** com **Arquitetura Hexagonal** e **Angular 17+** com **Signals**. Suporte completo a **Docker**, observabilidade distribuída com **Loki**, **Promtail** e **Grafana**, com rastreabilidade ponta a ponta via **Correlation ID**.

## 📁 Estrutura do Repositório

```
cdb-calculator/
├── docker-compose.yml                                # Orquestração de containers
├── README.md                                          # Documentação do projeto
├── .gitignore                                         # Git ignore patterns
│
├── backend/                                           # .NET 8 Backend (Arquitetura Hexagonal)
│   ├── CdbCalc.sln                                    # Solução Visual Studio
│   ├── Dockerfile                                     # Multi-stage build .NET
│   │
│   └── src/
│       ├── CdbCalc.Domain/                            # Core Domain (Sem dependências externas)
│       │   ├── CdbCalc.Domain.csproj
│       │   ├── ICdbCalculatorUseCase.cs               # Interface (Port)
│       │   └── CdbCalculation.cs                      # Modelo de domínio + lógica de cálculo
│       │
│       ├── CdbCalc.Application/                       # Camada de Aplicação (Use Cases)
│       │   ├── CdbCalc.Application.csproj
│       │   └── CdbCalculatorUseCase.cs                # Implementação do use case
│       │
│       └── CdbCalc.Adapters.Primary.WebApi/           # Adapter Primário (HTTP/REST)
│           ├── CdbCalc.Adapters.Primary.WebApi.csproj
│           ├── Program.cs                             # Configuração da aplicação
│           ├── appsettings.json                       # Configurações (Production)
│           ├── appsettings.Development.json           # Configurações (Development)
│           │
│           ├── Controllers/
│           │   └── CdbCalculatorController.cs         # Endpoint POST /api/cdbcalculator/calculate
│           │
│           ├── Dtos/
│           │   └── CdbCalculationDtos.cs              # Request/Response DTOs
│           │
│           └── Middlewares/
│               └── CorrelationIdMiddleware.cs         # Correlation ID injection
│
│   └── tests/
│       └── CdbCalc.Application.Tests/
│           ├── CdbCalc.Application.Tests.csproj
│           └── CdbCalculatorUseCaseTests.cs           # 10+ testes com >90% cobertura
│
├── frontend/                                          # Angular 17+ Frontend (Standalone)
│   ├── Dockerfile                                     # Multi-stage Node.js + Nginx
│   ├── nginx.conf                                     # Configuração Nginx (SPA fallback)
│   ├── package.json                                   # Dependências Node.js
│   ├── angular.json                                   # Configuração Angular CLI
│   ├── tsconfig.json                                  # TypeScript config
│   ├── tsconfig.app.json                              # TypeScript app config
│   ├── tsconfig.spec.json                             # TypeScript test config
│   ├── karma.conf.js                                  # Karma test runner
│   │
│   └── src/
│       ├── main.ts                                    # Bootstrap (providers: interceptors)
│       ├── index.html                                 # Arquivo HTML entry
│       ├── test.ts                                    # Test entry
│       │
│       └── app/
│           ├── components/
│           │   └── calculator.component.html
│           │   └── calculator.component.css
│           │   └── calculator.component.ts            # Standalone component + Signals
│           │
│           ├── services/
│           │   ├── cdb-calculator.service.ts          # HTTP service
│           │   ├── cdb-calculator.service.spec.ts     # Testes
│           │   ├── loading.service.ts                 # Loading state (Signal)
│           │   └── loading.service.spec.ts            # Testes
│           │
│           └── interceptors/
│               ├── correlation-id.interceptor.ts      # Injeta X-Correlation-ID
│               ├── correlation-id.interceptor.spec.ts # Testes
│               ├── loading.interceptor.ts             # Gerencia LoadingService
│               └── loading.interceptor.spec.ts        # Testes
│
└── observability/
    ├── grafana/
    │   ├── datasources/
    │   │   ├── loki.yaml                              # Datasource Loki
    │   └── dashboards/
    │       └── dashboard-provider.yaml                # Dashboard 
    provisioning
    │
    ├── loki/
        └── loki-config.yaml                       # Loki configuration
    └── promtail/
        └── config.yaml                                # Promtail logs scraping
```

## 🚀 Como Executar Localmente

### Pré-requisitos

- Docker & Docker Compose (versão 3.8+)
- **Opcional para testes/desenvolvimento:** 
  - .NET 8 SDK
  - Node.js 20+
  - Angular CLI 17+

### Executar via Docker Compose (Recomendado)

```bash
# Clone ou navegue ao repositório
cd cdb-calculator

# Construa e inicie todos os serviços
docker-compose up -d --build

# Aguarde os containers iniciarem (2-3 minutos)

# Para parar o containers/ambiente
docker-compose down
```

### URLs de Acesso Local

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **Frontend (SPA)** | http://localhost | - |
| **Backend (Swagger/OpenAPI)** | http://localhost:8080/swagger | - |
| **Grafana (Logs)** | http://localhost:3000 | admin / admin |
| **Loki (Logs API)** | http://localhost:3100 | - |

### Executar Testes Localmente (sem Docker)

#### Backend (.NET 8)

```bash
cd backend

# Restaurar dependências
dotnet restore

# Executar testes unitários
dotnet test

# Executar testes com cobertura no powershell
dotnet test tests/CdbCalc.Application.Tests/CdbCalc.Application.Tests.csproj "/p:CollectCoverage=true" "/p:VsTestUseMSBuildOutput=false" "/tl:false"

# Executar testes com cobertura no Git Bash:
dotnet test tests/CdbCalc.Application.Tests/CdbCalc.Application.Tests.csproj -p:CollectCoverage=true -p:VsTestUseMSBuildOutput=false -tl:false
```

**Esperado:** Testes passam com cobertura >90%

#### Frontend (Angular 17+)

```bash
cd frontend

# Instalar dependências
npm install

# Executar testes unitários (Karma + Jasmine)
npm test

# Executar testes com cobertura
npm test -- --code-coverage

# Executar lint
npx ng lint
```

**Esperado:** Todos os testes passam, cobertura >80%

## 📊 Regras de Negócio (CDB B3)

### Inputs
- **Valor Inicial (VI):** Valor em R$ > 0
- **Prazo (Meses):** Número de meses > 1

### Cálculo Mensal Composto

Para cada mês $m$ de 1 a $Meses$:

$$VF_m = VI_m \times [1 + (CDI \times TB)]$$

Onde:
- $CDI = 0.009$ (0.9% ao mês)
- $TB = 1.08$ (Taxa de Bracket = 108%)
- $VI_{m+1} = VF_m$ (valor final do mês atual é o inicial do próximo)

### Imposto de Renda Regressivo (sobre o lucro)

O IR é calculado sobre o lucro: $Lucro = VF_{final} - VI_{inicial}$

| Prazo | Alíquota |
|-------|----------|
| Até 6 meses | 22.5% |
| Até 12 meses | 20% |
| Até 24 meses | 17.5% |
| Acima de 24 meses | 15% |

### Fórmulas Finais

- **Valor Bruto:** $VF_{final}$ (resultado da composição)
- **Imposto:** $IR = Lucro \times Alíquota$
- **Valor Líquido:** $VL = VF_{final} - IR$

### Exemplo Prático

**Entrada:** VI = R$ 1.000, Prazo = 12 meses

**Cálculo mensal:** $(1 + 0.009 \times 1.08)^{12} = 1.10902...$

**Resultado:**
- Valor Bruto: R$ 1.109,02
- Lucro: R$ 109,02
- IR (20%): R$ 21,80
- Valor Líquido: R$ 1.087,22

## 🏗️ Arquitetura Backend (Hexagonal/Ports & Adapters)

### Camadas

#### 1. **CdbCalc.Domain** (Core/Innermost)
- **Sem dependências externas** (apenas C# standard library)
- Contém lógica pura de domínio
- `ICdbCalculatorUseCase` (Port/Interface)
- `CdbCalculation` (Model + Logic)
- **Princípios SOLID:** ISP (segregação), DIP (dependência de abstrações)

#### 2. **CdbCalc.Application** (Camada de Aplicação)
- Depende apenas de `CdbCalc.Domain`
- Implementa use cases: `CdbCalculatorUseCase`
- Orquestra chamadas ao domínio
- **Princípio SOLID:** SRP (responsabilidade única)

#### 3. **CdbCalc.Adapters.Primary.WebApi** (Adapter Primário)
- Depende de `Domain` e `Application`
- Expõe HTTP REST API
- Controllers, Middlewares, Swagger
- **Tecnologias:**
  - ASP.NET Core 8
  - Serilog (JSON structured logging)
  - Swagger/OpenAPI
  - Middleware: CorrelationId

### Padrões Implementados

- **Dependency Injection:** ASP.NET Core native
- **Correlation ID:** Middleware que captura/gera UUID via header `X-Correlation-ID`
- **Structured Logging:** Serilog com JSON formatter, inclui `CorrelationId` em todo log
- **Health Checks:** Endpoint `/health`

## 🌐 Arquitetura Frontend (Angular 17+ Standalone)

### Tecnologias

- **Framework:** Angular 17+
- **State Management:** Signals (não Redux/NgRx)
- **HTTP:** HttpClient + Interceptors funcionais
- **Validação:** Reactive Forms + Validators
- **Testes:** Jasmine + Karma
- **Build:** Angular CLI + webpack

### Componentes

#### 1. **CalculatorComponent** (Standalone)
- Formulário reativo com validação nativa
- Botão desabilitado se inválido
- Card com resultados (Bruto, IR, Líquido)
- Input: Valor Inicial, Prazo
- Output: Card com `CdbCalculationResponse`

#### 2. **Interceptors**

**CorrelationIdInterceptor:**
- Injeta `X-Correlation-ID` em cada request
- Reutiliza ID da sessão (sessionStorage)
- Gera novo UUID se não existir

**LoadingInterceptor:**
- Atualiza `LoadingService.isLoading` Signal
- Mostra overlay com spinner durante requests
- Bloqueia UI (opção: desabilitar inputs)

#### 3. **Services**

**CdbCalculatorService:**
- POST `/api/cdbcalculator/calculate`
- Observable<CdbCalculationResponse>

**LoadingService:**
- `isLoading = signal(false)` (reativo)
- Shared entre interceptor e componente

## 🐳 Infraestrutura Docker

### Backend Dockerfile (Multi-stage)

```dockerfile
# Stage 1: Builder
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine
WORKDIR /src
# Restaurar, compilar, publicar

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
COPY --from=builder /app/publish .
EXPOSE 8080
```

**Otimizações:**
- Alpine Linux (pequeno)
- Multi-stage (apenas runtime no final)
- Health check integrado

### Frontend Dockerfile (Multi-stage)

```dockerfile
# Stage 1: Builder
FROM node:20-alpine
# npm ci, npm run build

# Stage 2: Nginx
FROM nginx:alpine
COPY --from=builder /app/dist/cdb-calculator-frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
```

**Features:**
- Nginx alpine (leve)
- SPA fallback: `try_files $uri $uri/ /index.html`
- Gzip compression
- Cache-Control headers (1 year para assets)
- Proxy para `/api/` → backend

### Docker Compose (Orquestração)

**Serviços:**

1. **cdb-backend:**
   - Build: `./backend/Dockerfile`
   - Port: 8080
   - Healthcheck: GET `/health`
   - Network: cdb-network

2. **cdb-frontend:**
   - Build: `./frontend/Dockerfile`
   - Port: 80
   - Dependência: cdb-backend
   - Network: cdb-network

3. **loki:**
   - Image: grafana/loki:latest
   - Port: 3100
   - Volume: loki-storage
   - Network: cdb-network

4. **promtail:**
   - Image: grafana/promtail:latest
   - Volumes: `/var/log`, `/var/lib/docker/containers`
   - Scrape: Docker containers + files
   - Push para: Loki:3100
   - Network: cdb-network

5. **grafana:**
   - Image: grafana/grafana:latest
   - Port: 3000
   - Datasource: Loki (pré-configurado)
   - Admin: admin/admin
   - Volume: provisioning (dashboards, datasources)
   - Network: cdb-network

## 📝 Observabilidade & Rastreabilidade

### Correlation ID (Ponta a Ponta)

1. **Frontend:** `CorrelationIdInterceptor` injeta `X-Correlation-ID` em cada request
2. **Backend:** `CorrelationIdMiddleware` captura ou gera novo UUID
3. **Logging:** Serilog injeta `CorrelationId` no LogContext (propriedade estruturada)
4. **Response:** Header `X-Correlation-ID` retornado ao cliente
5. **Loki:** Logs contêm campo `CorrelationId` (JSON estruturado)

### Como Buscar Logs no Grafana

#### Passo 1: Acessar Grafana
```
http://localhost:3000
Login: admin / admin
```

#### Passo 2: Ir para Explore (Loki datasource)
```
Menu > Explore > Datasource: Loki
```

#### Passo 3: Query por Correlation ID
```
{job="cdb-backend"} | json CorrelationId="<YOUR_CORRELATION_ID>"
```

**Exemplo completo:**
```
{job="cdb-backend"} 
  | json 
  | CorrelationId="550e8400-e29b-41d4-a716-446655440000"
```

#### Passo 4: Ver Timeline
- Todos os logs da transação aparecem em ordem
- Campos: timestamp, level, message, CorrelationId
- Pode fazer drill-down em logs específicos

### Promtail Scraping

**Fontes:**
- Docker containers (via socket)
- Arquivos `/var/log`
- Syslog (porta 1514)

**Labels automáticos:**
- `job=cdb-backend` para container cdb-backend
- `container_name` do Docker
- `service` do docker-compose

## ✅ Testes

### Backend (.NET 8, xUnit)

**Arquivo:** `backend/tests/CdbCalc.Application.Tests/CdbCalculatorUseCaseTests.cs`

**Testes (~15 cases, >90% cobertura):**

1. **Validação de Entrada:**
   - Zero, negativo, inválido → exceção

2. **Faixas de IR (todas as alíquotas):**
   - Até 6 meses: 22.5%
   - 7-12 meses: 20%
   - 13-24 meses: 17.5%
   - 25+ meses: 15%

3. **Precisão de Cálculo:**
   - Valores arredondados a 2 casas decimais
   - NetValue = GrossValue - IncomeTax
   - Composição mensal correta

4. **Casos Edge:**
   - 1 mês
   - Valores grandes (100k+)
   - Múltiplos períodos

**Executar:**
```bash
cd backend
dotnet test
dotnet test /p:CollectCoverage=true
```

### Frontend (Angular 17+, Jasmine/Karma)

**Arquivos:**
- `src/app/services/cdb-calculator.service.spec.ts`
- `src/app/services/loading.service.spec.ts`
- `src/app/interceptors/correlation-id.interceptor.spec.ts`
- `src/app/interceptors/loading.interceptor.spec.ts`

**Testes (~20 cases, >80% cobertura):**

1. **CdbCalculatorService:**
   - POST request correto
   - Parsing response
   - Error handling

2. **LoadingService:**
   - Signal reativo
   - Update state

3. **CorrelationIdInterceptor:**
   - Injeta header
   - Reutiliza ID da sessão

4. **LoadingInterceptor:**
   - Ativa durante request
   - Desativa no fim

**Executar:**
```bash
cd frontend
npm install
npm test
npm test -- --code-coverage
```

## 📦 Dependências

### Backend (.NET 8)

```csproj
Serilog.AspNetCore 8.0.1
Serilog.Formatting.Json 1.1.0
Swashbuckle.AspNetCore 6.4.6
xunit 2.6.6
Microsoft.NET.Test.Sdk 17.8.2
```

### Frontend (Angular 17+)

```json
@angular/core 17.0.0
@angular/common 17.0.0
@angular/forms 17.0.0
@angular/platform-browser 17.0.0
rxjs 7.8.0
uuid 9.0.1
```

## 🔧 Variáveis de Ambiente

### Backend

```
ASPNETCORE_ENVIRONMENT=Production (ou Development)
ASPNETCORE_URLS=http://+:8080
```

### Frontend

Sem env vars críticas (URLs codificadas, alteráveis via nginx.conf)

### Docker Compose

```
GF_SECURITY_ADMIN_USER=admin
GF_SECURITY_ADMIN_PASSWORD=admin
```

## 🛠️ Desenvolvimento Local

### Backend (.NET 8)

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/CdbCalc.Adapters.Primary.WebApi
```

API disponível em: `http://localhost:5000` (Kestrel)

### Frontend (Angular 17+)

```bash
cd frontend
npm install
npm start
```

SPA disponível em: `http://localhost:4200` (dev server)

## 📋 Checklist de Entrega

- ✅ Arquitetura Hexagonal (Domain, Application, WebApi)
- ✅ Cálculo CDB B3 com composição mensal
- ✅ IR regressivo (4 faixas)
- ✅ Swagger/OpenAPI
- ✅ Correlation ID Middleware (UUID, LogContext, Response Header)
- ✅ Serilog JSON estruturado
- ✅ Testes unitários xUnit (>90% cobertura, todas as faixas IR)
- ✅ Angular 17+ Standalone (sem NgModule)
- ✅ Signals para state management
- ✅ Interceptors (Correlation ID, Loading)
- ✅ Validação de formulário + botão desabilitado
- ✅ Card de resultados (Bruto, IR, Líquido)
- ✅ Testes Jasmine/Karma (>80% cobertura)
- ✅ Multi-stage Dockerfile (.NET Alpine, Node.js+Nginx)
- ✅ Docker Compose (backend, frontend, Loki, Promtail, Grafana)
- ✅ Nginx SPA fallback + proxy /api/
- ✅ Observabilidade (Correlation ID em Grafana/Loki)
- ✅ README completo com instruções

## 📄 Licença

MIT

---

**Desenvolvido com ❤️ em .NET 8 & Angular 17+ com Arquitetura Hexagonal e Observabilidade Distribuída**

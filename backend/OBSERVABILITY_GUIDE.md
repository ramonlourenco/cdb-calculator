# 📊 Guia de Observabilidade com Serilog + Loki + Grafana

## 🎯 Visão Geral

Este guia documenta a implementação completa de observabilidade distribuída em uma API .NET 8 com:
- **Serilog**: Logs estruturados em JSON
- **CorrelationId**: Rastreamento de requisições
- **HttpLoggingMiddleware**: Captura automática de Request/Response
- **Loki**: Armazenamento de logs
- **Grafana**: Visualização e consultas

---

## 📦 Dependências Necessárias

Adicione ao `CdbCalc.Adapters.Primary.WebApi.csproj`:

```xml
<ItemGroup>
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.1" />
    <PackageReference Include="Serilog.Formatting.Compact" Version="2.2.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
    <!-- Opcional: Para enviar logs diretamente para Loki -->
    <PackageReference Include="Serilog.Sinks.Grafana.Loki" Version="8.3.1" />
</ItemGroup>
```

---

## 🔧 Configuração do Program.cs

### 1. Configuração Base (Console JSON)

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()              // ← Crítico para CorrelationId
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "CdbCalc.WebApi")
    
    // Reduzir ruído de logs do framework
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    
    // Logs de aplicação em nível Information
    .MinimumLevel.Override("CdbCalc", LogEventLevel.Information)
    
    // Output em JSON estruturado
    .WriteTo.Console(new JsonFormatter(renderMessage: true))
    
    .CreateLogger();

builder.Host.UseSerilog();
```

### 2. Registrar Middlewares (Ordem Importa!)

```csharp
// No método app.Build() ou antes de mapear rotas:
app.UseMiddleware<CorrelationIdMiddleware>();  // 1º: Gerar CorrelationId
app.UseMiddleware<HttpLoggingMiddleware>();    // 2º: Capturar Request/Response
```

---

## 📝 Exemplos de Logs Estruturados

### Log Automático do Middleware

Quando uma requisição é processada:

```json
{
  "Timestamp": "2024-06-02T10:30:45.1234567Z",
  "Level": "Information",
  "MessageTemplate": "HTTP {Method} {Path} completado com status {StatusCode}. Detalhes: {@HttpTransaction}",
  "Properties": {
    "CorrelationId": "550e8400-e29b-41d4-a716-446655440000",
    "HttpMethod": "POST",
    "Path": "/api/cdbcalculator/calculate",
    "Method": "POST",
    "StatusCode": 200,
    "ContentType": "application/json",
    "HttpTransaction": {
      "Method": "POST",
      "Path": "/api/cdbcalculator/calculate",
      "StatusCode": 200,
      "ContentType": "application/json",
      "CorrelationId": "550e8400-e29b-41d4-a716-446655440000",
      "Timestamp": "2024-06-02T10:30:45.1234567Z",
      "Request": {
        "initialValue": 10000.00,
        "months": 12
      },
      "Response": {
        "initialValue": 10000.00,
        "months": 12,
        "grossValue": 10225.50,
        "incomeTax": 113.45,
        "netValue": 10112.05
      }
    }
  }
}
```

### Log Manual no Controller

```csharp
using Serilog;
using Serilog.Context;

[ApiController]
[Route("api/[controller]")]
public class CdbCalculatorController : ControllerBase
{
    private readonly ICdbCalculatorUseCase _useCase;
    private readonly ILogger _logger = Log.ForContext<CdbCalculatorController>();

    public CdbCalculatorController(ICdbCalculatorUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] CdbCalculationDto dto)
    {
        try
        {
            // Log estruturado com payload desestruturado
            _logger.Information(
                "Iniciando cálculo CDB com entrada: {@Input}",
                new { dto.InitialValue, dto.Months }
            );

            var result = _useCase.Execute(dto.InitialValue, dto.Months);

            // Log estruturado com resultado
            _logger.Information(
                "Cálculo CDB finalizado com sucesso. Resultado: {@Result}",
                result
            );

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.Warning(
                ex,
                "Validação falhou para cálculo CDB com entrada: {@Input}",
                new { dto.InitialValue, dto.Months }
            );
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Erro ao processar cálculo CDB com entrada: {@Input}",
                new { dto.InitialValue, dto.Months }
            );
            return StatusCode(500, "Erro interno");
        }
    }
}
```

---

## 🐳 Configuração do Docker Compose com Loki

### docker-compose.yml (Adições necessárias)

```yaml
services:
  # ... seus serviços existentes ...

  loki:
    image: grafana/loki:latest
    container_name: loki
    ports:
      - "3100:3100"
    volumes:
      - ./observability/loki/loki-config.yaml:/etc/loki/local-config.yaml
      - loki-data:/loki
    command: -config.file=/etc/loki/local-config.yaml
    networks:
      - cdb-network

  promtail:
    image: grafana/promtail:latest
    container_name: promtail
    volumes:
      - ./observability/promtail/promtail-config.yaml:/etc/promtail/config.yaml
      - /var/lib/docker/containers:/var/lib/docker/containers:ro
      - /var/run/docker.sock:/var/run/docker.sock
    command: -config.file=/etc/promtail/config.yaml
    networks:
      - cdb-network
    depends_on:
      - loki

  grafana:
    image: grafana/grafana:latest
    container_name: grafana
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - ./observability/grafana/datasources/loki.yaml:/etc/grafana/provisioning/datasources/loki.yaml
      - grafana-data:/var/lib/grafana
    networks:
      - cdb-network
    depends_on:
      - loki

volumes:
  loki-data:
  grafana-data:

networks:
  cdb-network:
```

---

## 🔍 Queries Loki no Grafana

### 1. Logs de uma Requisição Específica

```logql
{job="docker"} | json | CorrelationId="550e8400-e29b-41d4-a716-446655440000"
```

### 2. Todas as Requisições para uma Rota

```logql
{job="docker"} | json | Path="/api/cdbcalculator/calculate"
```

### 3. Erros em uma Janela de Tempo

```logql
{job="docker"} | json | Level="Error" | timestamp > "2024-06-02T10:00:00Z"
```

### 4. Requisições com Status 500

```logql
{job="docker"} | json | StatusCode="500" | __error__=""
```

### 5. Tempo de Resposta por Rota

```logql
{job="docker"} | json | Path=~"/api/.*" | duration > 1000
```

---

## 💡 Boas Práticas

### 1. Use @ para Desestruturação

```csharp
// ✅ CORRETO - Propriedades se tornam campos JSON de primeiro nível
_logger.Information("Operação concluída: {@Result}", result);

// ❌ ERRADO - Apenas uma string, difícil filtrar no Loki
_logger.Information($"Operação concluída: {result}");
```

### 2. Evite Logs de Health Check

```csharp
// HttpLoggingMiddleware automaticamente ignora:
// - /health
// - /health/live
// - /health/ready
// - /metrics
// - /api/health
```

### 3. Use CorrelationId para Rastreamento Distribuído

```csharp
// Header da requisição:
// X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000

// Todos os logs desta requisição terão:
// "CorrelationId": "550e8400-e29b-41d4-a716-446655440000"

// Facilita correlacionar logs entre múltiplos serviços
```

### 4. Enriqueça o LogContext com Dados Úteis

```csharp
using (LogContext.PushProperty("UserId", user.Id))
using (LogContext.PushProperty("TenantId", tenant.Id))
{
    _logger.Information("Operação executada");
    // Log incluirá: UserId, TenantId automaticamente
}
```

### 5. Estruture Exceções

```csharp
try
{
    // ... código ...
}
catch (Exception ex)
{
    _logger.Error(
        ex,
        "Operação falhou com entrada: {@Input}",
        new { param1, param2 }
    );
}
```

---

## 🚀 Performance e Otimizações

### 1. HttpLoggingMiddleware utiliza MemoryStream

- **Não aloca strings desnecessariamente**
- **Reutiliza buffers eficientemente**
- **Não bloqueia o cliente durante captura**

### 2. Exclusões de Rotas

- Health checks são ignorados automaticamente
- Reduz overhead de CPU/Memória
- Mantém Grafana/Loki limpo de ruído

### 3. Enriquecer no LogContext

- Uma única atribuição de CorrelationId
- Todas as operações subsequentes herdam o valor
- Sem necessidade de passar parâmetros

---

## 📊 Visualização no Grafana

### Painel Recomendado: Taxa de Erros por Rota

```logql
sum(rate({job="docker"} | json | Level="Error" [5m])) by (Path)
```

### Painel: Latência P95 por Rota

```logql
histogram_quantile(0.95, sum(rate({job="docker"} | json [5m])) by (Path))
```

### Painel: Distribuição de Status Codes

```logql
sum(count_over_time({job="docker"} | json [5m])) by (StatusCode)
```

---

## ✅ Checklist de Configuração

- [ ] Serilog configurado com JsonFormatter
- [ ] Enrich.FromLogContext() ativado
- [ ] CorrelationIdMiddleware registrado
- [ ] HttpLoggingMiddleware registrado
- [ ] Logs de erro estruturados com exceções
- [ ] Health checks excluídos de logging
- [ ] Loki e Promtail configurados
- [ ] Datasource Loki adicionado ao Grafana
- [ ] Painéis Grafana criados para monitoramento
- [ ] Testes end-to-end validados

---

## 🔗 Referências

- [Serilog Documentation](https://github.com/serilog/serilog/wiki)
- [Grafana Loki](https://grafana.com/oss/loki/)
- [LogQL Query Language](https://grafana.com/docs/loki/latest/logql/)
- [Structured Logging Best Practices](https://github.com/serilog/serilog/wiki)

using CdbCalc.Adapters.Primary.WebApi.Middlewares;
using CdbCalc.Application;
using CdbCalc.Domain;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIGURAÇÃO ENTERPRISE DO SERILOG ==========
// Configurar Serilog com logs estruturados em JSON
Log.Logger = new LoggerConfiguration()
    // Definir o nível mínimo global
    .MinimumLevel.Information()
    
    // Enriquecer os logs com contexto estruturado
    .Enrich.FromLogContext()  // Inclui dados do LogContext (ex: CorrelationId)
    .Enrich.WithProperty("Application", "CdbCalc.WebApi") // Identificador da aplicação
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    
    // Aplicar filtros (overrides) para reduzir ruído de frameworks
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)           // Framework do .NET: apenas Warning+
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("CdbCalc", LogEventLevel.Information)         // Aplicação própria: Information+
    
    // Writer: Console com formato JSON estruturado (compatível com Loki/Grafana)
    .WriteTo.Console(new JsonFormatter(renderMessage: true))
    
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CDB Calculator API",
        Version = "v1",
        Description = "API for calculating CDB B3 investments with income tax considerations"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<ICdbCalculatorUseCase, CdbCalculatorUseCase>();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// ========== MIDDLEWARES DE OBSERVABILIDADE ==========
// Ordem importa! CorrelationId deve vir antes de HttpLogging
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<HttpLoggingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
